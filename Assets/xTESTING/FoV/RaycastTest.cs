using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

namespace Game
{
    public enum ColliderType
    {
        Box = 0,
        Circle = 1,
        Capsule = 2,
        Polygon = 3,   // closed polygon (PolygonCollider2D)
        Edge = 4,      // open polyline (EdgeCollider2D)
        Composite = 5, // treated as a closed set of edges
    }

    // This structure holds the minimal data needed for our raycast tests.
    // For simple colliders (box, circle, capsule) we use dedicated fields.
    // For polygon, edge, and composite colliders we record the start index/count into a vertex array.
    public struct ColliderData
    {
        public int type;
        // For BoxCollider2D and CircleCollider2D:
        public float2 center;
        public float rotation; // in radians (for box)
        public float2 size;    // for box (width, height)
        public float radius;   // for circle

        // For CapsuleCollider2D:
        public float2 capsuleA; // one end point in world space
        public float2 capsuleB; // other end point in world space
        public float capsuleRadius;

        // For PolygonCollider2D, EdgeCollider2D, and CompositeCollider2D:
        public int vertexStartIndex;
        public int vertexCount;
        public int isClosed; // 1 if the shape is closed (polygon, composite), 0 if open (edge)
    }

    public class RaycastTest : MonoBehaviour
    {
        public float _rayDistance = 10f;
        public Transform debugHitPoint;

        private void Update()
        {
            Raycast2D();
        }

        private void Raycast2D()
        {
            // Get colliders overlapping an area.
            Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, transform.lossyScale, transform.eulerAngles.z);

            // We accumulate collider data in a list (since some colliders—Composite—may produce multiple shape entries).
            List<ColliderData> colliderDataList = new List<ColliderData>();
            // For colliders that use per-vertex data (polygons, edges, composites), we accumulate vertices here.
            List<float2> vertexList = new List<float2>();

            foreach (var col in colliders)
            {
                // BOX
                if (col is BoxCollider2D box)
                {
                    ColliderData data = new ColliderData();
                    data.type = (int)ColliderType.Box;
                    // Compute world center using the collider’s offset.
                    Vector2 offset = box.offset;
                    Vector2 worldCenter = (Vector2)box.transform.position +
                        (Vector2)(Quaternion.Euler(0, 0, box.transform.eulerAngles.z) * offset);
                    data.center = worldCenter;
                    data.rotation = math.radians(box.transform.eulerAngles.z);
                    // Adjust size by lossyScale.
                    Vector2 lossyScale = box.transform.lossyScale;
                    data.size = new float2(box.size.x * lossyScale.x, box.size.y * lossyScale.y);
                    data.radius = 0f;
                    colliderDataList.Add(data);
                }
                // CIRCLE
                else if (col is CircleCollider2D circle)
                {
                    ColliderData data = new ColliderData();
                    data.type = (int)ColliderType.Circle;
                    Vector2 offset = circle.offset;
                    Vector2 worldCenter = (Vector2)circle.transform.position +
                        (Vector2)(Quaternion.Euler(0, 0, circle.transform.eulerAngles.z) * offset);
                    data.center = worldCenter;
                    data.rotation = 0f;
                    data.size = float2.zero;
                    // Assume uniform scale (using the x component).
                    data.radius = circle.radius * circle.transform.lossyScale.x;
                    colliderDataList.Add(data);
                }
                // CAPSULE
                else if (col is CapsuleCollider2D capsule)
                {
                    ColliderData data = new ColliderData();
                    data.type = (int)ColliderType.Capsule;
                    // Compute world center.
                    Vector2 offset = capsule.offset;
                    Vector2 worldCenter = (Vector2)capsule.transform.position +
                        (Vector2)(Quaternion.Euler(0, 0, capsule.transform.eulerAngles.z) * offset);
                    // Get lossy scale.
                    Vector2 lossyScale = capsule.transform.lossyScale;
                    float width = capsule.size.x * lossyScale.x;
                    float height = capsule.size.y * lossyScale.y;
                    // CapsuleCollider2D.direction: 0 = horizontal, 1 = vertical.
                    if (capsule.direction == CapsuleDirection2D.Vertical)
                    {
                        data.capsuleRadius = width * 0.5f;
                        float segment = math.max(0f, height * 0.5f - data.capsuleRadius);
                        // Use transform.up for vertical orientation.
                        Vector2 up = capsule.transform.up;
                        data.capsuleA = worldCenter + up * segment;
                        data.capsuleB = worldCenter - up * segment;
                    }
                    else // Horizontal
                    {
                        data.capsuleRadius = height * 0.5f;
                        float segment = math.max(0f, width * 0.5f - data.capsuleRadius);
                        Vector2 right = capsule.transform.right;
                        data.capsuleA = worldCenter + right * segment;
                        data.capsuleB = worldCenter - right * segment;
                    }
                    colliderDataList.Add(data);
                }
                // POLYGON
                else if (col is PolygonCollider2D poly)
                {
                    ColliderData data = new ColliderData();
                    data.type = (int)ColliderType.Polygon;
                    data.vertexStartIndex = vertexList.Count;
                    // poly.points are in local space; transform them to world space.
                    for (int i = 0; i < poly.points.Length; i++)
                    {
                        Vector2 worldPt = poly.transform.TransformPoint(poly.points[i]);
                        vertexList.Add(worldPt);
                    }
                    data.vertexCount = poly.points.Length;
                    data.isClosed = 1; // PolygonCollider2D is a closed shape.
                    colliderDataList.Add(data);
                }
                // EDGE
                else if (col is EdgeCollider2D edge)
                {
                    ColliderData data = new ColliderData();
                    data.type = (int)ColliderType.Edge;
                    data.vertexStartIndex = vertexList.Count;
                    for (int i = 0; i < edge.points.Length; i++)
                    {
                        Vector2 worldPt = edge.transform.TransformPoint(edge.points[i]);
                        vertexList.Add(worldPt);
                    }
                    data.vertexCount = edge.points.Length;
                    data.isClosed = 0; // Edge is open.
                    colliderDataList.Add(data);
                }
                // COMPOSITE
                else if (col is CompositeCollider2D composite)
                {
                    // CompositeCollider2D may contain multiple paths. Add one ColliderData per path.
                    // IMPORTANT: To avoid applying the scale twice, do not use TransformPoint here.
                    for (int p = 0; p < composite.pathCount; p++)
                    {
                        int pointCount = composite.GetPathPointCount(p);
                        Vector2[] path = new Vector2[pointCount];
                        composite.GetPath(p, path);
                        ColliderData data = new ColliderData();
                        data.type = (int)ColliderType.Composite;
                        data.vertexStartIndex = vertexList.Count;
                        // Instead of using TransformPoint (which applies scale), only apply rotation and translation.
                        Vector2 compPos = composite.transform.position;
                        float compRot = composite.transform.eulerAngles.z;
                        Quaternion rot = Quaternion.Euler(0, 0, compRot);
                        for (int i = 0; i < path.Length; i++)
                        {
                            Vector2 worldPt = (Vector2)compPos + (Vector2)(rot * path[i]);
                            vertexList.Add(worldPt);
                        }
                        data.vertexCount = path.Length;
                        // Assume composite shapes are closed.
                        data.isClosed = 1;
                        colliderDataList.Add(data);
                    }
                }
                // FALLBACK: use bounds as a box.
                else
                {
                    ColliderData data = new ColliderData();
                    data.type = (int)ColliderType.Box;
                    data.center = new float2(col.bounds.center.x, col.bounds.center.y);
                    data.rotation = 0f;
                    data.size = new float2(col.bounds.size.x, col.bounds.size.y);
                    data.radius = 0f;
                    colliderDataList.Add(data);
                    Debug.LogError("Unsuported collider type");
                }
            }

            // Convert lists to NativeArrays.
            NativeArray<ColliderData> colliderDataArray = new NativeArray<ColliderData>(colliderDataList.Count, Allocator.TempJob);
            for (int i = 0; i < colliderDataList.Count; i++)
            {
                colliderDataArray[i] = colliderDataList[i];
            }
            NativeArray<float2> vertexArray = new NativeArray<float2>(vertexList.Count, Allocator.TempJob);
            for (int i = 0; i < vertexList.Count; i++)
            {
                vertexArray[i] = vertexList[i];
            }

            NativeList<float> hitDistances = new NativeList<float>(Allocator.TempJob);
            NativeArray<float> minDistance = new NativeArray<float>(1, Allocator.TempJob);
            minDistance[0] = _rayDistance;
            NativeArray<Vector2> hitPoint = new NativeArray<Vector2>(1, Allocator.TempJob);
            hitPoint[0] = Vector2.zero;

            Raycast2DJob job = new Raycast2DJob
            {
                rayOrigin = transform.position,
                rayDirection = transform.up,
                rayDistance = _rayDistance,
                colliderDataArray = colliderDataArray,
                vertexArray = vertexArray,
                hitResults = hitDistances,
                minHitDistance = minDistance,
                hitPoint = hitPoint,
            };

            job.Run();

            if (job.minHitDistance[0] != _rayDistance)
            {
                Debug.Log(job.minHitDistance[0].ToString("f3"));
            }

            debugHitPoint.position = job.hitPoint[0];

            colliderDataArray.Dispose();
            vertexArray.Dispose();
            hitDistances.Dispose();
            minDistance.Dispose();
            hitPoint.Dispose();
        }

        [BurstCompile]
        public struct Raycast2DJob : IJob
        {
            public Vector2 rayOrigin;
            public Vector2 rayDirection;
            public float rayDistance;

            public NativeArray<ColliderData> colliderDataArray;
            // Contains vertices for all polygon/edge/composite colliders.
            public NativeArray<float2> vertexArray;

            public NativeList<float> hitResults;
            public NativeArray<float> minHitDistance;
            public NativeArray<Vector2> hitPoint;

            public void Execute()
            {
                for (int i = 0; i < colliderDataArray.Length; i++)
                {
                    ColliderData data = colliderDataArray[i];
                    float newHitDistance = rayDistance;
                    Vector2 newHitPoint = Vector2.zero;
                    bool hit = false;

                    if (data.type == (int)ColliderType.Box)
                    {
                        hit = RayIntersectsBox(rayOrigin, rayDirection, rayDistance,
                                               data.center, data.rotation, data.size,
                                               out newHitDistance, out newHitPoint);
                    }
                    else if (data.type == (int)ColliderType.Circle)
                    {
                        hit = RayIntersectsCircle(rayOrigin, rayDirection, rayDistance,
                                                  data.center, data.radius,
                                                  out newHitDistance, out newHitPoint);
                    }
                    else if (data.type == (int)ColliderType.Capsule)
                    {
                        hit = RayIntersectsCapsule(rayOrigin, rayDirection, rayDistance,
                                                   data.capsuleA, data.capsuleB, data.capsuleRadius,
                                                   out newHitDistance, out newHitPoint);
                    }
                    else if (data.type == (int)ColliderType.Polygon ||
                             data.type == (int)ColliderType.Edge ||
                             data.type == (int)ColliderType.Composite)
                    {
                        hit = RayIntersectsPolygon(vertexArray, data.vertexStartIndex, data.vertexCount, data.isClosed,
                                                    rayOrigin, rayDirection, rayDistance,
                                                    out newHitDistance, out newHitPoint);
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
}
