using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Profiling;
using Game.Utility;

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
        Unsuported = 6,
    }

    // This structure holds the minimal data needed for our raycast tests.
    // For simple colliders (box, circle, capsule) we use dedicated fields.
    // For polygon, edge, and composite colliders we record the start index/count into a vertex array.
    public struct ColliderDataUnprepared
    {
        ///public int type;
        public ColliderType typeEnum;
        // For BoxCollider2D and CircleCollider2D:
        ///public float2 center;
        public Vector3 posWorld;
        public Vector2 offsetLoc;

        ///public float rotationRad; // in radians (for box)
        public float rotWorld;

        ///public float2 size;    // for box (width, height)
        public Vector3 lossyScale;
        public Vector2 sizeLoc;

        ///public float radius;   // for circle
        public float radiusLoc;

        // For CapsuleCollider2D:
        ///public float2 capsuleA; // one end point in world space
        ///public float2 capsuleB; // other end point in world space
        ///public float capsuleRadius;
        public CapsuleDirection2D capsuleDirEnum;
        public Vector3 capsuleTransUp;
        public Vector3 capsuleTransRight;

        // For PolygonCollider2D, EdgeCollider2D, and CompositeCollider2D:
        public int vertexStartIndex;
        public int vertexCount;
        ///public int isClosed; // 1 if the shape is closed (polygon, composite), 0 if open (edge)
        public bool isClosedBool;
    }

    public struct ColliderDataReady
    {
        public int type;
        // For BoxCollider2D and CircleCollider2D:
        public float2 center;
        public float rotationRad; // in radians (for box)
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

        private Collider2D[] colliders;
        private Vector2[] points;

        private void Update()
        {
            Raycast2D();
        }

        private void Raycast2D()
        {
                    Profiler.BeginSample("amigus1-1 over");
            // Get colliders overlapping an area.
            colliders = Physics2D.OverlapCircleAll(transform.position, transform.lossyScale.x/2);
            Profiler.EndSample();

                    Profiler.BeginSample("amigus1-2 list1");
            // We accumulate collider data in a list (since some colliders—Composite—may produce multiple shape entries).
            NativeList<ColliderDataUnprepared> colliderDatasUnprepared = new NativeList<ColliderDataUnprepared>(colliders.Length, Allocator.TempJob);
            Profiler.EndSample();

                    Profiler.BeginSample("amigus1-3 list2");
            // For colliders that use per-vertex data (polygons, edges, composites), we accumulate vertices here.
            NativeList<Vector2> verticesUnprepared = new NativeList<Vector2>(colliders.Length * 5, Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-4 dataUnpare");
            foreach (var col in colliders)
            {
                // BOX
                if (col is BoxCollider2D box)
                {
                    //Profiler.BeginSample("amigus box");
                    ColliderDataUnprepared data = new()
                    {
                        typeEnum = ColliderType.Box,
                        posWorld = box.transform.position,
                        rotWorld = box.transform.eulerAngles.z,
                        offsetLoc = box.offset,
                        lossyScale = box.transform.lossyScale,
                        sizeLoc = box.size
                    };

                    colliderDatasUnprepared.Add(data);
                    //Profiler.EndSample();
                }
                // CIRCLE
                else if (col is CircleCollider2D circle)
                {
                    //Profiler.BeginSample("amigus circle");
                    ColliderDataUnprepared data = new()
                    {
                        typeEnum = ColliderType.Circle,
                        posWorld = circle.transform.position,
                        rotWorld = circle.transform.eulerAngles.z,
                        offsetLoc = circle.offset,
                        lossyScale = circle.transform.localScale,
                        radiusLoc = circle.radius
                    };
                    

                    colliderDatasUnprepared.Add(data);
                    //Profiler.EndSample();
                }
                // CAPSULE
                else if (col is CapsuleCollider2D capsule)
                {
                    //Profiler.BeginSample("amigus capsule");
                    ColliderDataUnprepared data = new()
                    {
                        typeEnum = ColliderType.Capsule,
                        offsetLoc = capsule.offset,
                        posWorld = capsule.transform.position,
                        rotWorld = capsule.transform.eulerAngles.z,
                        lossyScale = capsule.transform.lossyScale,
                        sizeLoc = capsule.size,
                        capsuleDirEnum = capsule.direction,
                        capsuleTransUp = capsule.transform.up,
                        capsuleTransRight = capsule.transform.right
                    };

                    colliderDatasUnprepared.Add(data);
                    //Profiler.EndSample();
                }
                // POLYGON
                else if (col is PolygonCollider2D poly)
                {
                    //Profiler.BeginSample("amigus polygon");
                    Vector2[] points = poly.points;

                    for (int i = 0; i < points.Length; i++)
                    {
                        verticesUnprepared.Add(points[i]);
                    }

                    ColliderDataUnprepared data = new()
                    {
                        typeEnum = ColliderType.Polygon,
                        vertexStartIndex = verticesUnprepared.Length,
                        posWorld = poly.transform.position,
                        rotWorld = poly.transform.eulerAngles.z,
                        lossyScale = poly.transform.lossyScale,
                        vertexCount = points.Length,
                        isClosedBool = true
                    };

                    colliderDatasUnprepared.Add(data);
                    //Profiler.EndSample();
                }
                // EDGE
                else if (col is EdgeCollider2D edge)
                {
                    //Profiler.BeginSample("amigus edge");

                    Vector2[] points = edge.points;
                    for (int i = 0; i < points.Length; i++)
                    {
                        verticesUnprepared.Add(points[i]);
                    }

                    ColliderDataUnprepared data = new ColliderDataUnprepared()
                    {
                        typeEnum = ColliderType.Edge,
                        vertexStartIndex = verticesUnprepared.Length,
                        posWorld = edge.transform.position,
                        rotWorld = edge.transform.eulerAngles.z,
                        lossyScale = edge.transform.lossyScale,
                        vertexCount = points.Length,
                        isClosedBool = false,
                    };

                    colliderDatasUnprepared.Add(data);
                    //Profiler.EndSample();
                }
                // COMPOSITE
                else if (col is CompositeCollider2D composite)
                {
                    //Profiler.BeginSample("amigus composite");
                    for (int p = 0; p < composite.pathCount; p++)
                    {
                        int pointCount = composite.GetPathPointCount(p);
                        Vector2[] path = new Vector2[pointCount];
                        composite.GetPath(p, path);

                        for (int i = 0; i < path.Length; i++)
                        {
                            verticesUnprepared.Add(path[i]);
                        }

                        ColliderDataUnprepared data = new ColliderDataUnprepared()
                        {
                            typeEnum = ColliderType.Composite,
                            vertexStartIndex = verticesUnprepared.Length,
                            posWorld = composite.transform.position,
                            rotWorld = composite.transform.eulerAngles.z,
                            vertexCount = path.Length,
                            isClosedBool = true,
                        };

                        colliderDatasUnprepared.Add(data);
                    }

                   // Profiler.EndSample();
                }
                // FALLBACK: use bounds as a box.
                else
                {
                    ColliderDataUnprepared data = new ColliderDataUnprepared()
                    {
                        typeEnum = ColliderType.Unsuported,
                        posWorld = col.bounds.center,
                        sizeLoc = col.bounds.size,
                    };

                    colliderDatasUnprepared.Add(data);
                    Debug.LogError("Unsuported collider type");
                }
            }
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-5 dataPrepare list1");
            //Preparing data job
            NativeArray<ColliderDataReady> colliderDatasReady = new(colliderDatasUnprepared.Length, Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-6 dataPrepare list2");
            NativeArray<float2> verticesReady = new(verticesUnprepared.Length, Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-7-1 dataPrepare job");
            PrepareColliderDatasJob prepareJob = new PrepareColliderDatasJob
            {
                datasUnprep = colliderDatasUnprepared,
                vertsUnprep = verticesUnprepared,
                datasRdy = colliderDatasReady,
                vertsRdy = verticesReady,
            };
            Profiler.EndSample();

            //Profiler.BeginSample("amigus1-7-2 dataPrepare job");
            //JobHandle prepareJobHandle = prepareJob.Schedule(colliderDatasUnprepared.Length, 5);
            //Profiler.EndSample();

            Profiler.BeginSample("amigus1-7-3 dataPrepare job");
            prepareJob.Run(colliderDatasReady.Length);
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-8 dataPrepare dispose");
            colliderDatasUnprepared.Dispose();
            verticesUnprepared.Dispose();
            Profiler.EndSample();

            //Ray job

            Profiler.BeginSample("amigus1-9 rayJob dists list");
            NativeList<float> hitDistances = new NativeList<float>(Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("amigus2-1 rayJob min array");
            NativeArray<float> minDistance = new NativeArray<float>(1, Allocator.TempJob);
            minDistance[0] = _rayDistance;
            Profiler.EndSample();

            Profiler.BeginSample("amigus2-2 rayJob hitPoint array");
            NativeArray<Vector2> hitPoint = new NativeArray<Vector2>(1, Allocator.TempJob);
            hitPoint[0] = Vector2.zero;
            Profiler.EndSample();

            //TODO Job for convert ColliderDataUnprepared in to ColliderDataReady

            Profiler.BeginSample("amigus2-3 rayJob");
            Raycast2DJob raycastJob = new Raycast2DJob
            {
                rayOrigin = transform.position,
                rayDirection = transform.up,
                rayDistance = _rayDistance,
                colliderDataArray = colliderDatasReady,
                vertexArray = verticesReady,
                hitResults = hitDistances,
                minHitDistance = minDistance,
                hitPoint = hitPoint,
            };

            raycastJob.Run();
            Profiler.EndSample();

            Profiler.BeginSample("amigus2-4 pos set");
            debugHitPoint.position = raycastJob.hitPoint[0];
            Profiler.EndSample();

            Profiler.BeginSample("amigus2-5 rayJob dispose");
            colliderDatasReady.Dispose();
            verticesReady.Dispose();
            hitDistances.Dispose();
            minDistance.Dispose();
            hitPoint.Dispose();
            Profiler.EndSample();
        }

        [BurstCompile]
        public struct PrepareColliderDatasJob : IJobFor
        {
            [ReadOnly, NativeDisableParallelForRestriction] public NativeList<ColliderDataUnprepared> datasUnprep;
            [ReadOnly, NativeDisableParallelForRestriction] public NativeList<Vector2> vertsUnprep;

            [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ColliderDataReady> datasRdy;
            [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<float2> vertsRdy;

            public void Execute(int index)
            {
                // BOX
                if (datasUnprep[index].typeEnum == ColliderType.Box)
                {
                    ColliderDataReady data = new();
                    data.type = (int)ColliderType.Box;

                    // Compute world center using the collider’s offset.
                    data.center = (Vector2)datasUnprep[index].posWorld +
                        (Vector2)(Quaternion.Euler(0, 0, datasUnprep[index].rotWorld) * datasUnprep[index].offsetLoc);

                    ///data.center = worldCenter;
                    ///data.posWorld = box.transform.position;
                    ///data.rotWorld = box.transform.eulerAngles.z;
                    ///data.offsetLoc = box.offset;

                    data.rotationRad = math.radians(datasUnprep[index].rotWorld);

                    // Adjust size by lossyScale.
                    ///Vector2 lossyScale = box.transform.lossyScale;

                    data.size = new float2(datasUnprep[index].sizeLoc.x * datasUnprep[index].lossyScale.x,
                        datasUnprep[index].sizeLoc.y * datasUnprep[index].lossyScale.y);
                    ///data.sizeLoc = box.size;

                    ///data.radius = 0f;

                    datasRdy[index] = data;
                }
                // CIRCLE
                else if (datasUnprep[index].typeEnum == ColliderType.Circle)
                {
                    ColliderDataReady data = new();
                    data.type = (int)ColliderType.Circle;

                    data.center = (Vector2)datasUnprep[index].posWorld +
                        (Vector2)(Quaternion.Euler(0, 0, datasUnprep[index].rotWorld) * datasUnprep[index].offsetLoc);
                    ///data.posWorld = circle.transform.position;
                    ///data.rotWorld = circle.transform.eulerAngles.z;
                    ///data.offsetLoc = circle.offset;


                    ///data.size = float2.zero;

                    // Assume uniform scale (using the x component).
                    data.radius = datasUnprep[index].radiusLoc * datasUnprep[index].lossyScale.x;
                    ///data.lossyScale = circle.transform.localScale;
                    ///data.radiusLoc = circle.radius;

                    ///datasUnprep.Add(data);
                    datasRdy[index] = data;
                }
                // CAPSULE
                else if (datasUnprep[index].typeEnum == ColliderType.Capsule)
                {
                    ColliderDataReady data = new();
                    data.type = (int)ColliderType.Capsule;

                    // Compute world center.
                    ///Vector2 offset = capsule.offset;
                    ///data.offsetLoc = capsule.offset;

                    Vector2 worldCenter = (Vector2)datasUnprep[index].posWorld +
                        (Vector2)(Quaternion.Euler(0, 0, datasUnprep[index].rotWorld) * datasUnprep[index].offsetLoc);
                    ///data.posWorld = capsule.transform.position;
                    ///data.rotWorld = capsule.transform.eulerAngles.z;

                    // Get lossy scale.
                    ///Vector2 lossyScale = datasUnprep[index].lossyScale;
                    float width = datasUnprep[index].sizeLoc.x * datasUnprep[index].lossyScale.x;
                    float height = datasUnprep[index].sizeLoc.y * datasUnprep[index].lossyScale.y;
                    ///data.lossyScale = capsule.transform.lossyScale;
                    ///data.sizeLoc = capsule.size;

                    // CapsuleCollider2D.direction: 0 = horizontal, 1 = vertical.
                    if (datasUnprep[index].capsuleDirEnum == CapsuleDirection2D.Vertical)
                    {
                        data.capsuleRadius = width * 0.5f;
                        float segment = math.max(0f, height * 0.5f - data.capsuleRadius);
                        // Use transform.up for vertical orientation.
                        ///Vector2 up = capsule.transform.up;
                        data.capsuleA = worldCenter + (Vector2)datasUnprep[index].capsuleTransUp * segment;
                        data.capsuleB = worldCenter - (Vector2)datasUnprep[index].capsuleTransUp * segment;
                    }
                    else // Horizontal
                    {
                        data.capsuleRadius = height * 0.5f;
                        float segment = math.max(0f, width * 0.5f - data.capsuleRadius);
                        ///Vector2 right = datasUnprep[index].capsuleTransRight;
                        data.capsuleA = worldCenter + (Vector2)datasUnprep[index].capsuleTransRight * segment;
                        data.capsuleB = worldCenter - (Vector2)datasUnprep[index].capsuleTransRight * segment;
                    }
                    ///data.capsuleDirEnum = capsule.direction;
                    ///data.capsuleTransUp = capsule.transform.up;
                    ///data.capsuleTransRight = capsule.transform.right;

                    //datasUnprep.Add(data);
                    datasRdy[index] = data;
                }
                // POLYGON
                else if (datasUnprep[index].typeEnum == ColliderType.Polygon)
                {
                    ColliderDataReady data = new();
                    data.type = (int)ColliderType.Polygon;

                    data.vertexStartIndex = datasUnprep[index].vertexStartIndex;

                    // poly.points are in local space; transform them to world space.
                    ///Vector2[] polyPoint = poly.points;

                    Vector2 worldPos = datasUnprep[index].posWorld;
                    float worldAngle = datasUnprep[index].rotWorld;
                    Vector2 loosyScale = datasUnprep[index].lossyScale;
                    ///data.posWorld = poly.transform.position;
                    ///data.rotWorld = poly.transform.eulerAngles.z;
                    ///data.lossyScale = poly.transform.lossyScale;

                    for (int i = datasUnprep[index].vertexStartIndex; 
                        i < datasUnprep[index].vertexStartIndex + datasUnprep[index].vertexCount; i++)
                    {
                        ///Vector2 worldPt = poly.transform.TransformPoint(polyPoint[i]);
                        vertsRdy[i] = Utils.TransformPoint(vertsUnprep[i], worldPos, worldAngle, loosyScale);

                        ///vertsUnprep.Add(poly.points[i]);
                    }
                    data.vertexCount = datasUnprep[index].vertexCount;

                    ///data.isClosed = 1; // PolygonCollider2D is a closed shape.
                    data.isClosed = datasUnprep[index].isClosedBool ? 1 : 0;

                    ///datasUnprep.Add(data);
                    datasRdy[index] = data;
                }
                // EDGE
                else if (datasUnprep[index].typeEnum == ColliderType.Edge)
                {
                    ColliderDataReady data = new();
                    data.type = (int)ColliderType.Edge;

                    data.vertexStartIndex = datasUnprep[index].vertexStartIndex;
                    for (int i = datasUnprep[index].vertexStartIndex; 
                        i < datasUnprep[index].vertexStartIndex + datasUnprep[index].vertexCount; i++)
                    {
                        ///Vector2 worldPt = edge.transform.TransformPoint(edge.points[i]);
                        vertsRdy[i] = Utils.TransformPoint(vertsUnprep[i], datasUnprep[index].posWorld,
                            datasUnprep[index].rotWorld, datasUnprep[index].lossyScale);

                        //vertsUnprep.Add(edge.points[i]);
                    }
                    ///data.posWorld = edge.transform.position;
                    ///data.rotWorld = edge.transform.eulerAngles.z;
                    ///data.lossyScale = edge.transform.lossyScale;

                    data.vertexCount = datasUnprep[index].vertexCount;

                    data.isClosed = datasUnprep[index].isClosedBool ? 1 : 0; // Edge is open.
                    ///data.isClosedBool = false;

                    //datasUnprep.Add(data);
                    datasRdy[index] = data;
                }
                // COMPOSITE
                else if (datasUnprep[index].typeEnum == ColliderType.Composite)
                {
                    // CompositeCollider2D may contain multiple paths. Add one ColliderData per path.
                    // IMPORTANT: To avoid applying the scale twice, do not use TransformPoint here.
                    ///for (int p = 0; p < composite.pathCount; p++)
                    ///{

                    ColliderDataReady data = new();
                    data.type = (int)ColliderType.Composite;

                    ///int pointCount = composite.GetPathPointCount(p);
                    ///Vector2[] path = new Vector2[pointCount];
                    ///composite.GetPath(p, path);

                    data.vertexStartIndex = datasUnprep[index].vertexStartIndex;
                    // Instead of using TransformPoint (which applies scale), only apply rotation and translation.
                    ///Vector2 compPos = composite.transform.position;
                    ///float compRot = composite.transform.eulerAngles.z;
                    Quaternion rot = Quaternion.Euler(0, 0, datasUnprep[index].rotWorld);
                    ///data.posWorld = composite.transform.position;
                    ///data.rotWorld = composite.transform.eulerAngles.z;

                    for (int i = datasUnprep[index].vertexStartIndex;
                        i < datasUnprep[index].vertexStartIndex + datasUnprep[index].vertexCount; i++)
                    {
                        vertsRdy[i] = (Vector2)datasUnprep[index].posWorld + 
                            (Vector2)(rot * vertsUnprep[i]);
                        ///vertsUnprep.Add(path[i]);
                    }
                    data.vertexCount = datasUnprep[index].vertexCount;

                    // Assume composite shapes are closed.
                    data.isClosed = datasUnprep[index].isClosedBool ? 1 : 0;
                    ///data.isClosedBool = ;

                    //datasUnprep.Add(data);
                    datasRdy[index] = data;
                    ///}
                }
                // FALLBACK: use bounds as a box.
                else
                {
                    ColliderDataReady data = new();
                    data.type = (int)ColliderType.Box;

                    data.center = new float2(datasUnprep[index].posWorld.x, datasUnprep[index].posWorld.y);
                    ///data.posWorld = col.bounds.center;

                    ///data.rotationRad = 0f;

                    data.size = new float2(datasUnprep[index].sizeLoc.x, datasUnprep[index].sizeLoc.y);
                    ///data.sizeLoc = col.bounds.size;

                    ///data.radius = 0f;

                    ///datasUnprep.Add(data);
                    datasRdy[index] = data;
                }
            }
        }

        [BurstCompile]
        public struct Raycast2DJob : IJob
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

                    if (data.type == (int)ColliderType.Box)
                    {
                        hit = RayIntersectsBox(rayOrigin, rayDirection, rayDistance,
                                               data.center, data.rotationRad, data.size,
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
