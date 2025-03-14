using CodeMonkey.Utils;
using Game.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.tvOS;

namespace Game.Physics
{
    //[BurstCompile(FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance, DisableDirectCall = true)]
    public struct Raycast2DWithMeshJob : IJobParallelFor
    {
        //public Vector2 rayOrigin;
        //public float rayDistance;
        //public int rayCount;
        //public float fovAnlge;
        //public float worldAngleAdd;

        // int - EntityId
        [ReadOnly] public NativeHashMap<int, FovEntityData> fovEntityDatas;
        // int1 - EntityId, int2 ColliderId
        [ReadOnly] public NativeMultiHashMap<int, int> entitiesColliders;
        // int - colliderId
        [ReadOnly] public NativeHashMap<int, ColliderDataReady> colliderDataArray;
        // Contains vertices for all polygon/edge/composite colliders.
        [ReadOnly] public NativeArray<float2> vertexArray;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<Vector3> verticies;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<int> triangles;

        public void Execute(int index)
        {
            int entityId = 0;

            foreach (var kvp in fovEntityDatas)
            {
                int vertsCount = kvp.Value.rayCount + 2;
                if (index < vertsCount + kvp.Value.vertciesBeforeCount)
                {
                    entityId = kvp.Key;
                    break;
                }
            }

            int rayCount = fovEntityDatas[entityId].rayCount;
            Vector2 rayOrigin = fovEntityDatas[entityId].rayOrigin;
            int verticiesBeforeCount = fovEntityDatas[entityId].vertciesBeforeCount;

            if (index == verticiesBeforeCount || // first vert
                index == verticiesBeforeCount + rayCount + 1) // last vert
            {
                int firstLastVertexIndex = index + fovEntityDatas[entityId].vertciesBeforeCount;
                verticies[firstLastVertexIndex] = rayOrigin;
                return;
            }


            float fovAnlge = fovEntityDatas[entityId].fovAnlge;
            float worldAngleAdd = fovEntityDatas[entityId].worldAngleAdd;
            float rayDistance = fovEntityDatas[entityId].rayDistance;
            int rayBeforeCount = fovEntityDatas[entityId].rayBeforeCount;

            float startAngle = (fovAnlge / 2) + 90;
            float currentAngle = startAngle - (((float)index / (float)rayCount) * fovAnlge);

            Vector2 rayDirection = UtilsClass.GetVectorFromAngle(currentAngle + worldAngleAdd);

            float minHitDistance = float.MaxValue;
            Vector2 minHitPoint = Vector2.zero;
            bool hitOnce = false;


            if (!entitiesColliders.TryGetFirstValue(entityId, out int currentColliderId,
                out NativeMultiHashMapIterator<int> iterator))
            {
                return;
            }

            do
            {
                ColliderDataReady data = colliderDataArray[currentColliderId];
                float newHitDistance = float.MaxValue;
                Vector2 newHitPoint = Vector2.zero;
                bool hit = false;

                switch (data.type)
                {
                    case (int)ColliderType.Box:
                        hit = RayIntersectsBox(rayOrigin, rayDirection, rayDistance,
                            data.center, data.rotationRad, data.size, out newHitDistance,
                            out newHitPoint);
                        break;

                    case (int)ColliderType.Circle:
                        hit = RayIntersectsCircle(rayOrigin, rayDirection, rayDistance, data.center,
                            data.radius, out newHitDistance, out newHitPoint);
                        break;

                    case (int)ColliderType.Capsule:
                        hit = RayIntersectsCapsule(rayOrigin, rayDirection, rayDistance, data.capsuleA,
                            data.capsuleB, data.capsuleRadius, out newHitDistance, out newHitPoint);
                        break;

                    case (int)ColliderType.Polygon:
                    case (int)ColliderType.Edge:
                    case (int)ColliderType.Composite:
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
                    }
                }


            } while (entitiesColliders.TryGetNextValue(out currentColliderId, ref iterator));


            Vector3 vertex;
            if (hitOnce)
            {
                vertex = minHitPoint/* - rayOrigin*/;
                //vertex = Utils.RotateVector(vertex, -worldAngleAdd);
            }
            else
            {
                //Vector3 direction = UtilsClass.GetVectorFromAngle(currentAngle);
                //vertex = direction * rayDistance;

                Vector3 direction = UtilsClass.GetVectorFromAngle(currentAngle);
                vertex = direction * rayDistance;


                vertex = Utils.RotateVector(vertex, -worldAngleAdd);
                vertex = vertex + (Vector3)rayOrigin;
            }

            int vertexIndex = index + verticiesBeforeCount;
            verticies[vertexIndex] = vertex;

            int triIndex = index - 1;
            if (triIndex > 0)
            {
                int triangleIndex = (triIndex * 3) + (rayBeforeCount * 3);
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                //triangleIndex += 3;
            }
        }

        // --- Intersection routines ---

        private bool RayIntersectsBox(Vector2 rayOrigin, Vector2 rayDir, float rayDist,
                                        float2 boxCenter, float boxRotation, float2 boxSize,
                                        out float hitDistance, out Vector2 hitPoint)
        {
            // Transform the ray into the box's local space.
            float2 relativeOrigin = new float2(rayOrigin.x, rayOrigin.y) - boxCenter;
            float cos = math.cos(-boxRotation);
            float sin = math.sin(-boxRotation);
            float2 localOrigin = new float2(relativeOrigin.x * cos - relativeOrigin.y * sin,
                                           relativeOrigin.x * sin + relativeOrigin.y * cos);
            float2 localDir = new float2(rayDir.x * cos - rayDir.y * sin,
                                        rayDir.x * sin + rayDir.y * cos);

            float2 extents = boxSize * 0.5f;
            float2 invDir = new float2(1f / localDir.x, 1f / localDir.y);
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

        private bool RayIntersectsCircle(Vector2 rayOrigin, Vector2 rayDir, float rayDist,
                                           float2 circleCenter, float radius,
                                           out float hitDistance, out Vector2 hitPoint)
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

        private bool RayIntersectsCapsule(Vector2 rayOrigin, Vector2 rayDir, float rayDist,
                                            float2 A, float2 B, float radius,
                                            out float hitDistance, out Vector2 hitPoint)
        {
            bool hit = false;
            hitDistance = float.MaxValue;
            Vector2 tempHitPoint = Vector2.zero;

            float tA, tB, tRect;
            Vector2 ptA, ptB, ptRect;
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

        private bool RayIntersectsPolygon(NativeArray<float2> vertices, int startIndex, int count, int isClosed,
                                          Vector2 rayOrigin, Vector2 rayDir, float rayDist,
                                          out float hitDistance, out Vector2 hitPoint)
        {
            bool hitFound = false;
            hitDistance = rayDist;
            hitPoint = Vector2.zero;
            for (int i = 0; i < count - 1; i++)
            {
                float t;
                Vector2 pt;
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
                Vector2 pt;
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

        private bool RayIntersectsSegment(Vector2 rayOrigin, Vector2 rayDir, float rayDist,
                                            float2 p0, float2 p1, out float t, out Vector2 pt)
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

    }
}
