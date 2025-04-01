using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Physics
{
    [BurstCompile]
    public struct Raycasts2DJob : IJob
    {
        public Vector2 rayOrigin;
        public Vector2 rayDirection;
        public float rayDistance;

        public NativeArray<ColliderDataReady> colliderDataArray;
        // Contains vertices for all polygon/edge/composite colliders.
        public NativeArray<float2> vertexArray;

        public NativeList<float> hitResults;
        public NativeArray<float> minHitDistance;
        public NativeArray<Vector2> hitPoint;

        public void Execute()
        {
            for (int i = 0; i < colliderDataArray.Length; i++)
            {
                ColliderDataReady data = colliderDataArray[i];
                float newHitDistance = rayDistance;
                Vector2 newHitPoint = Vector2.zero;
                bool hit = false;

                switch (data.type)
                {
                    case (int)ColliderType.Box:
                        hit = RayIntersectsBox(rayOrigin, rayDirection, rayDistance,
                            data.center, data.rotationRad, data.size,out newHitDistance, 
                            out newHitPoint);
                        break;

                    case (int)ColliderType.Circle:
                        hit = RayIntersectsCircle(rayOrigin, rayDirection, rayDistance,data.center, 
                            data.radius,out newHitDistance, out newHitPoint);
                        break;

                    case (int)ColliderType.Capsule:
                        hit = RayIntersectsCapsule(rayOrigin, rayDirection, rayDistance,data.capsuleAOrBoundsPos, 
                            data.capsuleBOrBoundsSize, data.capsuleRadius, out newHitDistance, out newHitPoint);
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
                    hitResults.Add(newHitDistance);
                    if (newHitDistance < minHitDistance[0])
                    {
                        minHitDistance[0] = newHitDistance;
                        hitPoint[0] = newHitPoint;
                    }
                }
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
                hitDistance = 0f;
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
                hitDistance = 0f;
                hitPoint = float2.zero;
                return false;
            }

            float discr = b * b - c;
            if (discr < 0f)
            {
                hitDistance = 0f;
                hitPoint = float2.zero;
                return false;
            }

            float t = -b - math.sqrt(discr);
            if (t < 0f)
                t = 0f;
            if (t > rayDist)
            {
                hitDistance = 0f;
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
            hitDistance = rayDist;
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
                t = 0f;
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
            t = 0f;
            pt = float2.zero;
            return false;
        }
    }
}
