using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Game.Physics
{
    [BurstCompile(FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance, 
        DisableDirectCall = true)]
    public struct CreateEnemiesFieldOfViewJob : IJobParallelFor /*IJobFor*/
    {
        // int - EntityId
        [ReadOnly] public NativeHashMap<int, FovEntityData> fovEntityDatas;
        // int1 - EntityId, int2 ColliderId
        [ReadOnly] public NativeMultiHashMap<int, int> entitiesColliders;
        // int - colliderId
        [ReadOnly] public NativeHashMap<int, ColliderDataReady> colliderDataArray;
        // Contains vertices for all polygon/edge/composite colliders.
        [ReadOnly] public NativeArray<float2> vertexArray;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<float3> verticies;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<int> triangles;

        // int - cast enemyId, bool 
        public NativeHashMap<int, bool>.ParallelWriter enemiesPlayerHit;
        public NativeHashMap<EnemyHitData, bool>.ParallelWriter enemiesEnemyHit;

        public int playerLayer;
        public int enemyLayer;

        public void Execute(int index)
        {
            int entityId = 0;
            FovEntityData entityData = default;

            // Cache entity data so we don't repeatedly index the dictionary.
            foreach (var kvp in fovEntityDatas)
            {
                int vertsCount = kvp.Value.rayCount + 2;
                if (index >= kvp.Value.vertciesBeforeCount &&
                    index < kvp.Value.vertciesBeforeCount + vertsCount)
                {
                    entityId = kvp.Key;
                    entityData = kvp.Value;
                    break;
                }
            }

            int rayCount = entityData.rayCount;
            float2 rayOrigin = entityData.rayOrigin;
            int verticiesBeforeCount = entityData.vertciesBeforeCount;

            if (index == verticiesBeforeCount || // first vert
                index == verticiesBeforeCount + rayCount + 1) // last vert
            {
                int firstLastVertexIndex = index;
                verticies[firstLastVertexIndex] = new float3(rayOrigin, 0.0f);
                return;
            }

            float fovAnlge = entityData.fovAnlge;
            float worldAngleAdd = entityData.worldAngleAdd;
            float rayDistance = entityData.rayDistance;

            float startAngle = (fovAnlge / 2) + 90;

            int rayIndex = index - verticiesBeforeCount - 1;
            float currentAngle = startAngle - (((float)rayIndex / ((float)rayCount - 1.0f)) * fovAnlge);

            float2 rayDirection = GetVectorFromAngle(currentAngle + worldAngleAdd);

            float minHitDistance = float.MaxValue;
            float2 minHitPoint = float2.zero;
            bool isMinHitPlayer = false;
            bool hitOnce = false;

            #region StopRaycastLoop
            if (entitiesColliders.TryGetFirstValue(entityId, out int currentColliderId,
                out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    if (currentColliderId == FieldOfViewSystem.EMPTY_COLLIDER_ID)
                    {
                        continue;
                    }

                    ColliderDataReady data = colliderDataArray[currentColliderId];

                    if (data.layer == enemyLayer)
                    {
                        continue;
                    }

                    float newHitDistance = float.MaxValue;
                    float2 newHitPoint = float2.zero;
                    bool hit = false;

                    switch (data.type)
                    {
                        case ColliderType.Box:
                            hit = RayIntersectsBox(rayOrigin, rayDirection, rayDistance,
                                data.center, data.rotationRad, data.size, out newHitDistance,
                                out newHitPoint);
                            break;

                        case ColliderType.Circle:
                            hit = RayIntersectsCircle(rayOrigin, rayDirection, rayDistance, data.center,
                                data.radius, out newHitDistance, out newHitPoint);
                            break;

                        case ColliderType.Capsule:
                            hit = RayIntersectsCapsule(rayOrigin, rayDirection, rayDistance, data.capsuleAOrBoundsPos,
                                data.capsuleBOrBoundsSize, data.capsuleRadius, out newHitDistance, out newHitPoint);
                            break;

                        case ColliderType.Polygon:
                        case ColliderType.Edge:
                        case ColliderType.Composite:
                            bool boundsHit = RayIntersectsBox(rayOrigin, rayDirection, rayDistance,
                                data.capsuleAOrBoundsPos, 0, data.capsuleBOrBoundsSize, out newHitDistance,
                                out newHitPoint);

                            if (!boundsHit)
                                break;

                            hit = RayIntersectsPolygon(vertexArray, data.vertexStartIndex, data.vertexCount, data.isClosed,
                            rayOrigin, rayDirection, rayDistance, out newHitDistance, out newHitPoint);
                            break;
                    }

                    if (hit)
                    {
                        hitOnce = true;
                        if (newHitDistance < minHitDistance)
                        {
                            minHitDistance = newHitDistance;
                            minHitPoint = newHitPoint;
                            isMinHitPlayer = data.layer == playerLayer;
                        }
                    }
                }
                while (entitiesColliders.TryGetNextValue(out currentColliderId, ref iterator));
            }
            #endregion

            #region EnemiesLoop
            if (entitiesColliders.TryGetFirstValue(entityId, out currentColliderId,
                out iterator))
            {
                do
                {
                    if (currentColliderId == FieldOfViewSystem.EMPTY_COLLIDER_ID)
                    {
                        continue;
                    }

                    ColliderDataReady data = colliderDataArray[currentColliderId];

                    if (data.layer != enemyLayer)
                    {
                        continue;
                    }

                    float newHitDistance = float.MaxValue;
                    bool hit = false;

                    switch (data.type)
                    {
                        case ColliderType.Box:
                            hit = RayIntersectsBox(rayOrigin, rayDirection, rayDistance,
                                data.center, data.rotationRad, data.size, out newHitDistance,
                                out _);
                            break;

                        case ColliderType.Circle:
                            hit = RayIntersectsCircle(rayOrigin, rayDirection, rayDistance, data.center,
                                data.radius, out newHitDistance, out _);
                            break;

                        case ColliderType.Capsule:
                            hit = RayIntersectsCapsule(rayOrigin, rayDirection, rayDistance, data.capsuleAOrBoundsPos,
                                data.capsuleBOrBoundsSize, data.capsuleRadius, out newHitDistance, out _);
                            break;

                        case ColliderType.Polygon:
                        case ColliderType.Edge:
                        case ColliderType.Composite:
                            bool boundsHit = RayIntersectsBox(rayOrigin, rayDirection, rayDistance,
                                data.capsuleAOrBoundsPos, 0, data.capsuleBOrBoundsSize, out _,
                                out _);

                            if (!boundsHit)
                                break;

                                hit = RayIntersectsPolygon(vertexArray, data.vertexStartIndex, data.vertexCount, data.isClosed,
                                rayOrigin, rayDirection, rayDistance, out newHitDistance, out _);
                            break;
                    }

                    if (hit)
                    {
                        // Enemies detection
                        if (minHitDistance > newHitDistance)
                        {
                            enemiesEnemyHit.TryAdd(new(entityId, data.colliderId), false);
                        }
                    }
                }
                while (entitiesColliders.TryGetNextValue(out currentColliderId, ref iterator));
            }
            #endregion

            // Mesh
            float3 vertex;
            if (hitOnce)
            {
                vertex = new float3(minHitPoint, entityData.meshMoveZ);
            }
            else
            {
                float2 vector = math.normalize(rayDirection) * rayDistance;
                vertex = new float3(vector + rayOrigin, entityData.meshMoveZ);
            }

            int vertexIndex = index;
            verticies[vertexIndex] = vertex;

            int tirIndexMove = verticiesBeforeCount - entityData.rayBeforeCount;
            int triIndex = index - tirIndexMove - 1;
            if (triIndex > 0)
            {
                int triangleIndex = (triIndex * 3);
                triangles[triangleIndex] = verticiesBeforeCount;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;
            }

            // Player detection
            if (isMinHitPlayer)
            {
                enemiesPlayerHit.TryAdd(entityId, false);
            }
        }

        #region Intersection routines

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool RayIntersectsBox(float2 rayOrigin, float2 rayDir, float rayDist,
                                        float2 boxCenter, float boxRotation, float2 boxSize,
                                        out float hitDistance, out float2 hitPoint)
        {
            // Transform the ray into the box's local space.
            float2 relativeOrigin = new float2(rayOrigin.x, rayOrigin.y) - boxCenter;
            float cos = math.cos(-boxRotation);
            float sin = math.sin(-boxRotation);
            float2 localOrigin = new(relativeOrigin.x * cos - relativeOrigin.y * sin, 
                relativeOrigin.x * sin + relativeOrigin.y * cos);
            float2 localDir = new (rayDir.x * cos - rayDir.y * sin, rayDir.x * sin + rayDir.y * cos);

            float2 extents = boxSize * 0.5f;
            float2 invDir = new(1f / localDir.x, 1f / localDir.y);
            float2 tMin = (-extents - localOrigin) * invDir;
            float2 tMax = (extents - localOrigin) * invDir;
            float2 t1 = math.min(tMin, tMax);
            float2 t2 = math.max(tMin, tMax);
            float tNear = math.max(t1.x, t1.y);
            float tFar = math.min(t2.x, t2.y);

            if (tNear > tFar || tFar < 0f || tNear > rayDist)
            {
                hitDistance = float.MaxValue;
                hitPoint = float2.zero;
                return false;
            }

            hitDistance = tNear;
            float2 localHitPoint = localOrigin + localDir * tNear;
            float cosR = math.cos(boxRotation);
            float sinR = math.sin(boxRotation);
            hitPoint = new float2(localHitPoint.x * cosR - localHitPoint.y * sinR,
                                  localHitPoint.x * sinR + localHitPoint.y * cosR) + boxCenter;
            return true;
        }

        private bool RayIntersectsCircle(float2 rayOrigin, float2 rayDir, float rayDist,
                                           float2 circleCenter, float radius,
                                           out float hitDistance, out float2 hitPoint)
        {
            float2 m = new float2(rayOrigin.x, rayOrigin.y) - circleCenter;
            float b = math.dot(m, rayDir);
            float c = math.dot(m, m) - radius * radius;

            if (c > 0f && b > 0f)
            {
                hitDistance = float.MaxValue;
                hitPoint = float2.zero;
                return false;
            }

            float discr = b * b - c;
            if (discr < 0f)
            {
                hitDistance = float.MaxValue;
                hitPoint = float2.zero;
                return false;
            }

            float t = -b - math.sqrt(discr);
            if (t < 0f)
                t = 0f;
            if (t > rayDist)
            {
                hitDistance = float.MaxValue;
                hitPoint = float2.zero;
                return false;
            }
            hitDistance = t;
            hitPoint = rayOrigin + rayDir * t;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool RayIntersectsCapsule(float2 rayOrigin, float2 rayDir, float rayDist,
                                            float2 A, float2 B, float radius,
                                            out float hitDistance, out float2 hitPoint)
        {
            bool hit = false;
            hitDistance = float.MaxValue;
            float2 tempHitPoint = float2.zero;

            float tA, tB, tRect;
            float2 ptA, ptB, ptRect;
            bool hitA = RayIntersectsCircle(rayOrigin, rayDir, rayDist, A, radius, out tA, out ptA);
            bool hitB = RayIntersectsCircle(rayOrigin, rayDir, rayDist, B, radius, out tB, out ptB);

            // For the central part, define a box that represents the capsule's rectangle.
            float2 rectCenter = (A + B) * 0.5f;
            float rectRotation = math.atan2(B.y - A.y, B.x - A.x);
            float2 rectSize = new float2(math.distance(A, B), 2f * radius);
            bool hitRect = RayIntersectsBox(rayOrigin, rayDir, rayDist, rectCenter, rectRotation, rectSize, out tRect, out ptRect);

            if (hitA && tA < hitDistance) { hitDistance = tA; tempHitPoint = ptA; hit = true; }
            if (hitB && tB < hitDistance) { hitDistance = tB; tempHitPoint = ptB; hit = true; }
            if (hitRect && tRect < hitDistance) { hitDistance = tRect; tempHitPoint = ptRect; hit = true; }

            hitPoint = tempHitPoint;
            return hit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool RayIntersectsPolygon(NativeArray<float2> vertices, int startIndex, int count, int isClosed,
                                          float2 rayOrigin, float2 rayDir, float rayDist,
                                          out float hitDistance, out float2 hitPoint)
        {
            bool hitFound = false;
            hitDistance = rayDist;
            hitPoint = float2.zero;
            for (int i = 0; i < count - 1; i++)
            {
                float t;
                float2 pt;
                if (RayIntersectsSegment(rayOrigin, rayDir, rayDist, vertices[startIndex + i], vertices[startIndex + i + 1], out t, out pt))
                {
                    if (t < hitDistance)
                    {
                        hitDistance = t;
                        hitPoint = pt;
                        hitFound = true;
                    }
                }
            }
            if (isClosed == 1 && count > 2)
            {
                float t;
                float2 pt;
                if (RayIntersectsSegment(rayOrigin, rayDir, rayDist, vertices[startIndex + count - 1], vertices[startIndex], out t, out pt))
                {
                    if (t < hitDistance)
                    {
                        hitDistance = t;
                        hitPoint = pt;
                        hitFound = true;
                    }
                }
            }
            return hitFound;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool RayIntersectsSegment(float2 rayOrigin, float2 rayDir, float rayDist,
                                            float2 p0, float2 p1, out float t, out float2 pt)
        {
            float2 v = p1 - p0;
            float d = rayDir.x * v.y - rayDir.y * v.x;
            if (math.abs(d) < 1e-6f)
            {
                t = float.MaxValue;
                pt = float2.zero;
                return false;
            }
            t = ((p0.x - rayOrigin.x) * v.y - (p0.y - rayOrigin.y) * v.x) / d;
            float u = ((p0.x - rayOrigin.x) * rayDir.y - (p0.y - rayOrigin.y) * rayDir.x) / d;

            if (t >= 0f && t <= rayDist && u >= 0f && u <= 1f)
            {
                pt = rayOrigin + rayDir * t;
                return true;
            }
            t = float.MaxValue;
            pt = float2.zero;
            return false;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float2 GetVectorFromAngle(float angle)
        {
            // Convert angle (in degrees) to radians.
            float angleRad = math.radians(angle);
            return new float2(math.cos(angleRad), math.sin(angleRad));
        }
    }
}
