using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Game.Physics
{
    public class RaycastTest : MonoBehaviour
    {
        public float _rayDistance = 10f;
        public Transform debugHitPoint;

        private Collider2D[] colliders;
        private Vector2[] _pathPointsCompositeCache = new Vector2[10];

        private void Update()
        {
            Raycast2D();
        }

        private void Raycast2D()
        {
            Profiler.BeginSample("amigus1-1 over");
            colliders = Physics2D.OverlapCircleAll(transform.position, transform.lossyScale.x / 2);
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-2 list1");
            NativeList<ColliderDataUnprepared> datasUnprep = new(colliders.Length, Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-3 list2");
            NativeList<Vector2> vertsUnprep = new(colliders.Length * 5, Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-4 dataUnpare");
            foreach (var col in colliders)
            {
                switch (col)
                {
                    case BoxCollider2D box:
                        //Profiler.BeginSample("amigus box");
                        ColliderDataUnprepared boxData = new()
                        {
                            typeEnum = ColliderType.Box,
                            posWorld = box.transform.position,
                            rotWorld = box.transform.eulerAngles.z,
                            offsetLoc = box.offset,
                            lossyScale = box.transform.lossyScale,
                            sizeLoc = box.size
                        };
                        datasUnprep.Add(boxData);
                        //Profiler.EndSample();
                        break;

                    case CircleCollider2D circle:
                        //Profiler.BeginSample("amigus circle");
                        ColliderDataUnprepared circleData = new()
                        {
                            typeEnum = ColliderType.Circle,
                            posWorld = circle.transform.position,
                            rotWorld = circle.transform.eulerAngles.z,
                            offsetLoc = circle.offset,
                            lossyScale = circle.transform.localScale,
                            radiusLoc = circle.radius
                        };
                        datasUnprep.Add(circleData);
                        //Profiler.EndSample();
                        break;

                    case CapsuleCollider2D capsule:
                        //Profiler.BeginSample("amigus capsule");
                        ColliderDataUnprepared capsuleData = new()
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
                        datasUnprep.Add(capsuleData);
                        //Profiler.EndSample();
                        break;

                    case PolygonCollider2D poly:
                        //Profiler.BeginSample("amigus polygon 1");
                        Vector2[] points = poly.points;
                        //Profiler.EndSample();

                        //Profiler.BeginSample("amigus polygon 2");
                        ColliderDataUnprepared polyData = new()
                        {
                            typeEnum = ColliderType.Polygon,
                            vertexStartIndex = vertsUnprep.Length,
                            posWorld = poly.transform.position,
                            rotWorld = poly.transform.eulerAngles.z,
                            lossyScale = poly.transform.lossyScale,
                            vertexCount = points.Length,
                            isClosedBool = true
                        };
                        //Profiler.EndSample();

                        //Profiler.BeginSample("amigus polygon 3");
                        unsafe
                        {
                            fixed(Vector2* ptr = points)
                            {
                                vertsUnprep.AddRange(ptr, points.Length);
                            }
                        }

                        //Profiler.EndSample();
                        //Profiler.BeginSample("amigus polygon 4");
                        datasUnprep.Add(polyData);
                        //Profiler.EndSample();
                        break;

                    case EdgeCollider2D edge:
                        //Profiler.BeginSample("amigus edge");
                        Vector2[] edgePoints = edge.points;
                        ColliderDataUnprepared edgeData = new()
                        {
                            typeEnum = ColliderType.Edge,
                            vertexStartIndex = vertsUnprep.Length,
                            posWorld = edge.transform.position,
                            rotWorld = edge.transform.eulerAngles.z,
                            lossyScale = edge.transform.lossyScale,
                            vertexCount = edgePoints.Length,
                            isClosedBool = false,
                        };

                        unsafe
                        {
                            fixed (Vector2* ptr = edgePoints)
                            {
                                vertsUnprep.AddRange(ptr, edgePoints.Length);
                            }
                        }

                        datasUnprep.Add(edgeData);
                        //Profiler.EndSample();
                        break;

                    case CompositeCollider2D composite:
                        for (int p = 0; p < composite.pathCount; p++)
                        {
                            //Profiler.BeginSample("amigus composite 1");
                            int pointCount = composite.GetPathPointCount(p);
                            //Profiler.EndSample();

                            //Profiler.BeginSample("amigus composite 2");
                            ColliderDataUnprepared compositeData = new()
                            {
                                typeEnum = ColliderType.Composite,
                                vertexStartIndex = vertsUnprep.Length,
                                posWorld = composite.transform.position,
                                rotWorld = composite.transform.eulerAngles.z,
                                vertexCount = pointCount,
                                isClosedBool = true,
                            };
                            //Profiler.EndSample();

                            //Profiler.BeginSample("amigus composite 3");
                            unsafe
                            {
                                if (_pathPointsCompositeCache.Length < pointCount)
                                {
                                    _pathPointsCompositeCache = new Vector2[pointCount];
                                }
                                composite.GetPath(p, _pathPointsCompositeCache);
                                fixed (Vector2* pathPointsPtr = _pathPointsCompositeCache)
                                {
                                    vertsUnprep.AddRange(pathPointsPtr, pointCount);
                                }
                            }
                            
                            //Profiler.EndSample();

                            //Profiler.BeginSample("amigus composite 4");
                            datasUnprep.Add(compositeData);
                            //Profiler.EndSample();
                        }
                        break;

                    default:
                        ColliderDataUnprepared defaultData = new()
                        {
                            typeEnum = ColliderType.Unsuported,
                            posWorld = col.bounds.center,
                            sizeLoc = col.bounds.size,
                        };
                        datasUnprep.Add(defaultData);
                        Debug.LogError("Unsuported collider type");
                        break;

                }
            }
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-5 dataPrepare list1");
            NativeArray<ColliderDataReady> datasRdy = new(datasUnprep.Length, Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-6 dataPrepare list2");
            NativeArray<float2> vertsRdy = new(vertsUnprep.Length, Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-7-1 dataPrepare job");
            PrepareColliderDatasJob prepareJob = new PrepareColliderDatasJob
            {
                datasUnprep = datasUnprep,
                vertsUnprep = vertsUnprep,
                datasRdy = datasRdy,
                vertsRdy = vertsRdy,
            };
            Profiler.EndSample();

            //Profiler.BeginSample("amigus1-7-2 dataPrepare job");
            //JobHandle prepareJobHandle = prepareJob.Schedule(colliderDatasUnprepared.Length, 5);
            //Profiler.EndSample();

            Profiler.BeginSample("amigus1-7-3 dataPrepare job");
            prepareJob.Run(datasRdy.Length);
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-8 dataPrepare dispose");
            datasUnprep.Dispose();
            vertsUnprep.Dispose();
            Profiler.EndSample();

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

            Profiler.BeginSample("amigus2-3 rayJob");
            Raycasts2DJob raycastJob = new Raycasts2DJob
            {
                rayOrigin = transform.position,
                rayDirection = transform.up,
                rayDistance = _rayDistance,
                colliderDataArray = datasRdy,
                vertexArray = vertsRdy,
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
            datasRdy.Dispose();
            vertsRdy.Dispose();
            hitDistances.Dispose();
            minDistance.Dispose();
            hitPoint.Dispose();
            Profiler.EndSample();
        }


    }
}
