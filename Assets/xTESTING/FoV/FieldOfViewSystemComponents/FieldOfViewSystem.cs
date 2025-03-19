using Game.Utility.Globals;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Game.Physics
{
    public class FieldOfViewSystem : MonoBehaviour
    {
        // int1 - ColliderId, int2 - enter count
        private Dictionary<int, (Collider2D, int)> _collidersToUnprepare = new();

        // int1 - EntityId, int2 ColliderId
        private NativeMultiHashMap<int, int> _entitiesColliders;

        // int - EntityId
        private Dictionary<int, FieldOfViewEntity> _entityes = new();

        //// int - EntityId
        //private NativeHashMap<int, FovEntityData> _entityesData;

        private MeshFilter _meshFilter;
        private Mesh _mesh;

        private bool wasEntytiAddedLastTime = false;
        private bool wasEntytiesDicChanged = true;

        private LayerMask _allLayerMask;
        private ContactFilter2D _contactFilter;

        private Vector2[] _pathPointsCompositeCache = new Vector2[10];


        public LayerMask AllLayerMask => _allLayerMask;
        public ContactFilter2D ContactFilter => _contactFilter;

        private void Awake()
        {
            _entitiesColliders = new NativeMultiHashMap<int, int>(10, Allocator.Persistent);

            _allLayerMask = LayerMask.GetMask(Layers.Player, Layers.Obstacle, Layers.Enemy);
            _contactFilter = new ContactFilter2D
            {
                useTriggers = false,
                layerMask = _allLayerMask,
                useLayerMask = true
            };

            _meshFilter = GetComponent<MeshFilter>();
            _mesh = new Mesh();
            _meshFilter.mesh = _mesh;
        }

        private void OnDestroy()
        {
            if (_entitiesColliders.IsCreated)
                _entitiesColliders.Dispose();
        }

        private void Update()
        {
            UpdateView();
        }

        public void AddCollider(FieldOfViewEntity entity, Collider2D collider)
        {
            if (collider.isTrigger)
                return;

            int entityId = entity.GetInstanceID();
            int colliderId = collider.GetInstanceID();

            _entitiesColliders.Add(entityId, colliderId);

            if (_collidersToUnprepare.ContainsKey(colliderId))
            {
                var current = _collidersToUnprepare[colliderId];
                _collidersToUnprepare[colliderId] = (collider, current.Item2 + 1);
            }
            else
            {
                _collidersToUnprepare.Add(colliderId, (collider, 1));
            }

            if (_entityes.TryAdd(entityId, entity))
            {
                wasEntytiAddedLastTime = true;
                wasEntytiesDicChanged = true;
            }
        }

        public void RemoveCollider(FieldOfViewEntity entity, Collider2D collider)
        {
            if (collider.isTrigger)
                return;

            int entityId = entity.GetInstanceID();
            int colliderId = collider.GetInstanceID();

            bool removed = false;
            if (_entitiesColliders.TryGetFirstValue(entityId, out int currentColliderId,
                out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    if (currentColliderId == colliderId)
                    {
                        _entitiesColliders.Remove(iterator);
                        removed = true;
                        break;
                    }
                } while (_entitiesColliders.TryGetNextValue(out currentColliderId, ref iterator));
            }
            else
            {
                Debug.LogError("There is no entity to remove", entity);
                return;
            }

            if (removed && _entitiesColliders.CountValuesForKey(entityId) == 0)
            {
                _entityes.Remove(entityId);
                wasEntytiAddedLastTime = false;
                wasEntytiesDicChanged = true;
            }

            // Update the _collidersToUnprepare dictionary as before
            if (_collidersToUnprepare.ContainsKey(colliderId))
            {
                if (_collidersToUnprepare[colliderId].Item2 <= 1)
                {
                    if (_collidersToUnprepare[colliderId].Item2 <= 0)
                    {
                        Debug.LogError("_collidersToUnprepare[colliderId].Item2 <= 0 - entity ref", entity);
                        Debug.LogError("_collidersToUnprepare[colliderId].Item2 <= 0 - collider ref", collider);
                    }

                    _collidersToUnprepare.Remove(colliderId);
                }
                else
                {
                    var current = _collidersToUnprepare[colliderId];
                    _collidersToUnprepare[colliderId] = (collider, current.Item2 - 1);
                }
            }
            else
            {
                Debug.LogError("There is no collider to remove from _collidersToUnprepare - entity ref", entity);
                Debug.LogError("There is no collider to remove from _collidersToUnprepare - collider ref", collider);
            }
        }

        private void UpdateView()
        {
            Profiler.BeginSample("amigus1-2 datasUnprep alloc");
            NativeList<ColliderDataUnprepared> datasUnprep = new(_collidersToUnprepare.Count,
                Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-3 vertsUnprep alloc");
            NativeList<Vector2> vertsUnprep = new(_collidersToUnprepare.Count * 5, Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-4 dataUnpare");
            foreach (var pair in _collidersToUnprepare)
            {
                Collider2D col = pair.Value.Item1;
                Transform colTrans = col.transform;
                switch (col)
                {
                    case BoxCollider2D box:
                        Profiler.BeginSample("amigus1-4-1 dataUnpare box");

                        ColliderDataUnprepared boxData = new()
                        {
                            typeEnum = ColliderType.Box,
                            posWorld = colTrans.position,
                            rotWorld = colTrans.eulerAngles.z,
                            offsetLoc = box.offset,
                            lossyScale = colTrans.lossyScale,
                            sizeLoc = box.size,
                            colliderId = pair.Key,
                        };
                        datasUnprep.Add(boxData);
                        Profiler.EndSample();
                        break;

                    case CircleCollider2D circle:
                        Profiler.BeginSample("amigus1-4-2 dataUnpare circle");

                        ColliderDataUnprepared circleData = new()
                        {
                            typeEnum = ColliderType.Circle,
                            posWorld = colTrans.position,
                            rotWorld = colTrans.eulerAngles.z,
                            offsetLoc = circle.offset,
                            lossyScale = colTrans.localScale,
                            radiusLoc = circle.radius,
                            colliderId = pair.Key,
                        };
                        datasUnprep.Add(circleData);
                        Profiler.EndSample();
                        break;

                    case CapsuleCollider2D capsule:
                        Profiler.BeginSample("amigus1-4-3 dataUnpare capsule");
                        ColliderDataUnprepared capsuleData = new()
                        {
                            typeEnum = ColliderType.Capsule,
                            offsetLoc = capsule.offset,
                            posWorld = colTrans.position,
                            rotWorld = colTrans.eulerAngles.z,
                            lossyScale = colTrans.lossyScale,
                            sizeLoc = capsule.size,
                            capsuleDirEnum = capsule.direction,
                            capsuleTransUp = colTrans.up,
                            capsuleTransRight = colTrans.right,
                            colliderId = pair.Key,
                        };
                        datasUnprep.Add(capsuleData);
                        Profiler.EndSample();
                        break;

                    case PolygonCollider2D poly:
                        Profiler.BeginSample("amigus1-4-4 dataUnpare poly");
                        Vector2[] points = poly.points;
                        //Profiler.EndSample();

                        //Profiler.BeginSample("amigus polygon 2");
                        ColliderDataUnprepared polyData = new()
                        {
                            typeEnum = ColliderType.Polygon,
                            vertexStartIndex = vertsUnprep.Length,
                            posWorld = colTrans.position,
                            rotWorld = colTrans.eulerAngles.z,
                            lossyScale = colTrans.lossyScale,
                            vertexCount = points.Length,
                            colliderId = pair.Key,
                        };
                        //Profiler.EndSample();

                        //Profiler.BeginSample("amigus polygon 3");
                        unsafe
                        {
                            fixed (Vector2* ptr = points)
                            {
                                vertsUnprep.AddRange(ptr, points.Length);
                            }
                        }

                        //Profiler.EndSample();
                        //Profiler.BeginSample("amigus polygon 4");
                        datasUnprep.Add(polyData);
                        Profiler.EndSample();
                        break;

                    case EdgeCollider2D edge:
                        Profiler.BeginSample("amigus1-4-5 dataUnpare edge");
                        Vector2[] edgePoints = edge.points;
                        ColliderDataUnprepared edgeData = new()
                        {
                            typeEnum = ColliderType.Edge,
                            vertexStartIndex = vertsUnprep.Length,
                            posWorld = colTrans.position,
                            rotWorld = colTrans.eulerAngles.z,
                            lossyScale = colTrans.lossyScale,
                            vertexCount = edgePoints.Length,
                            colliderId = pair.Key,
                        };

                        unsafe
                        {
                            fixed (Vector2* ptr = edgePoints)
                            {
                                vertsUnprep.AddRange(ptr, edgePoints.Length);
                            }
                        }

                        datasUnprep.Add(edgeData);
                        Profiler.EndSample();
                        break;

                    case CompositeCollider2D composite:
                        Profiler.BeginSample("amigus1-4-6 dataUnpare composite");
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
                                posWorld = colTrans.position,
                                rotWorld = colTrans.eulerAngles.z,
                                vertexCount = pointCount,
                                colliderId = pair.Key,
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
                        Profiler.EndSample();
                        break;

                    default:
                        ColliderDataUnprepared defaultData = new()
                        {
                            typeEnum = ColliderType.Unsuported,
                            posWorld = col.bounds.center,
                            sizeLoc = col.bounds.size,
                            colliderId = pair.Key,
                        };
                        datasUnprep.Add(defaultData);
                        Debug.LogError("Unsuported collider type");
                        break;

                }
            }
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-5 datasRdy alloc");
            // int - colliderId
            NativeHashMap<int, ColliderDataReady> datasRdy = new(datasUnprep.Length, Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-6 vertsRdy alloc");
            NativeArray<float2> vertsRdy = new(vertsUnprep.Length, Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-7-1 dataPrepare job");
            PrepareColliderDatasJob prepareJob = new PrepareColliderDatasJob
            {
                datasUnprep = datasUnprep,
                vertsUnprep = vertsUnprep,
                datasRdy = datasRdy.AsParallelWriter(),
                vertsRdy = vertsRdy,
            };
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-7-2 dataPrepare job");
            prepareJob.Run(datasUnprep.Length);
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-8 dataPrepare dispose");
            datasUnprep.Dispose();
            vertsUnprep.Dispose();
            Profiler.EndSample();

            // int - entityId
            NativeHashMap<int, FovEntityData> fovDatas = new(_entityes.Count, Allocator.TempJob);
            int verticiesCount = 0;
            int rayCount = 0;

            foreach (var entity in _entityes)
            {
                fovDatas.Add(entity.Key, entity.Value.GetData(rayCount, verticiesCount));
                verticiesCount += fovDatas[entity.Key].rayCount + 2;
                rayCount += fovDatas[entity.Key].rayCount;
            }

            Profiler.BeginSample("amigus1-9 rayJob verticies array");
            NativeArray<Vector3> verticies = new(verticiesCount, Allocator.TempJob);
            //verticies[0] = Vector3.zero;
            Profiler.EndSample();

            Profiler.BeginSample("amigus2-2 rayJob triangles array");
            NativeArray<int> triangles = new(rayCount * 3, Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("amigus2-3 rayJob create");
            Raycast2DWithMeshJob raycastsJob = new Raycast2DWithMeshJob
            {
                fovEntityDatas = fovDatas,
                entitiesColliders = _entitiesColliders,
                colliderDataArray = datasRdy,
                vertexArray = vertsRdy,
                verticies = verticies,
                triangles = triangles,
            };
            Profiler.EndSample();

            Profiler.BeginSample("amigus2-4 rayJob run");
            int batchCount = Mathf.Max(1, verticiesCount / (JobsUtility.JobWorkerCount * 2));
            JobHandle raycastsJobHandle = raycastsJob.Schedule(verticiesCount, batchCount);
            raycastsJobHandle.Complete();
            //JobHandle raycastsJobHandle = raycastsJob.Schedule(verticiesCount, new JobHandle());
            //raycastsJobHandle.Complete();
            Profiler.EndSample();
            
            
            if (wasEntytiAddedLastTime)
            {
                Profiler.BeginSample("amigus2-6 mesh vertices");
                _mesh.SetVertices(verticies);
                Profiler.EndSample();

                if (wasEntytiesDicChanged)
                {
                    Profiler.BeginSample("amigus2-5 mesh triangles");
                    _mesh.SetIndices(triangles, MeshTopology.Triangles, 0, false, 0);
                    Profiler.EndSample();
                }
            }
            else
            {
                if (wasEntytiesDicChanged)
                {
                    Profiler.BeginSample("amigus2-5 mesh triangles");
                    _mesh.SetIndices(triangles, MeshTopology.Triangles, 0, false, 0);
                    Profiler.EndSample();
                }

                Profiler.BeginSample("amigus2-6 mesh vertices");
                _mesh.SetVertices(verticies);
                Profiler.EndSample();
            }
            wasEntytiesDicChanged = false;

            Profiler.BeginSample("amigus2-7 rayJob dispose");
            datasRdy.Dispose();
            vertsRdy.Dispose();
            verticies.Dispose();
            triangles.Dispose();
            fovDatas.Dispose();
            Profiler.EndSample();
        }
    }
}
