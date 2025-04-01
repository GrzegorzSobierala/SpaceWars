using Game.Utility.Globals;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using System;
using Game.Room.Enemy;
using System.Collections;

namespace Game.Physics
{
    [DefaultExecutionOrder(-100)]
    public class FieldOfViewSystem : MonoBehaviour
    {
        private event Action _onUpdateViewCompleted;

        [SerializeField] private float _meshMoveZStep = 0.001f;

        private MeshFilter _meshFilter;
        private Mesh _mesh;
        private VertexAttributeDescriptor _vertexAttributeDescriptor;
        private LayerMask _allLayerMask;
        private ContactFilter2D _contactFilter;

        private bool _wasEntitiesDicChanged = false;

        // int1 - ColliderId, int2 - enter count
        private Dictionary<int, (Collider2D, int)> _collidersToUnprepare = new();
        // int - EntityId
        private Dictionary<int, FieldOfViewEntity> _entityes = new();
        // int - enemy's colliderId
        private Dictionary<int, IGuardStateDetectable> _collidersDetectable = new();

        // int1 - EntityId, int2 ColliderId
        private NativeMultiHashMap<int, int> _entitiesColliders;

        private Vector2[] _pathPointsCompositeCache = new Vector2[10];

        private NativeList<ColliderDataUnprepared> _datasUnprep;
        private NativeList<float2> _vertsUnprep;
        //int - ColliderId
        private NativeHashMap<int, ColliderDataReady> _datasRdy;
        private NativeList<float2> _vertsRdy;
        //int - EntityId
        private NativeHashMap<int, FovEntityData> _fovDatas;
        // Vector3 - vertex world position
        private NativeArray<float3> _verticies;
        // int - vertexIndex 
        private NativeArray<int> _triangles;
        // int - cast enemyId
        private NativeHashMap<int, bool> _enemiesPlayerHit;
        private NativeHashMap<EnemyHitData, bool> _enemiesEnemyHit;

        private int _enemyLayer;

        public LayerMask AllLayerMask => _allLayerMask;
        public ContactFilter2D ContactFilter => _contactFilter;

        public const int _EMPTY_COLLIDER_ID = 0;

        private void Awake()
        {
            _allLayerMask = LayerMask.GetMask(Layers.Player, Layers.Obstacle, Layers.Enemy);
            _contactFilter = new ContactFilter2D
            {
                useTriggers = false,
                layerMask = _allLayerMask,
                useLayerMask = true
            };
            _enemyLayer = LayerMask.NameToLayer(Layers.Enemy);

            _meshFilter = GetComponent<MeshFilter>();
            _mesh = new Mesh();
            _mesh.MarkDynamic();

            _vertexAttributeDescriptor = new VertexAttributeDescriptor(
                VertexAttribute.Position,
                VertexAttributeFormat.Float32,
                3,  // Vector3 has 3 floats
                stream: 0
                );

            _meshFilter.mesh = _mesh;

            _entitiesColliders = new NativeMultiHashMap<int, int>(10, Allocator.Persistent);

            _datasUnprep = new(10, Allocator.Persistent);
            _vertsUnprep = new(50, Allocator.Persistent);
            _datasRdy = new(10, Allocator.Persistent);
            _vertsRdy = new(50, Allocator.Persistent);
            _fovDatas = new(5, Allocator.Persistent);
            _verticies = new(0, Allocator.Persistent);
            _triangles = new(0, Allocator.Persistent);
            _enemiesPlayerHit = new(5, Allocator.Persistent);
            _enemiesEnemyHit = new(25, Allocator.Persistent);
        }

        private void Start()
        {
            StartCoroutine(SubscribeCustomLoop());
        }

        private void OnDestroy()
        {
            if (_entitiesColliders.IsCreated)
                _entitiesColliders.Dispose();

            _datasUnprep.Dispose();
            _vertsUnprep.Dispose();
            _datasRdy.Dispose();
            _vertsRdy.Dispose();
            _fovDatas.Dispose();
            _verticies.Dispose();
            _triangles.Dispose();
            _enemiesPlayerHit.Dispose();
            _enemiesEnemyHit.Dispose();

            CustomPlayerLoopInjection.OnAfterPhysics2DUpdate -= ScheduleUpdateView;
            CustomPlayerLoopInjection.OnPostLateUpdateEnd -= CompliteUpdateView;
        }

        public void AddCollider(FieldOfViewEntity entity, Collider2D collider)
        {
            if (collider.isTrigger)
                return;

            int entityId = entity.GetInstanceID();
            int colliderId = collider.GetInstanceID();

            if (!_entityes.ContainsKey(entityId))
            {
                Debug.LogError("Add collider failed, entity isn't added", entity);
                return;
            }

            IncreaseCapasityOfEntitiesCollidersIfNeeded(10);
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

            if(collider.gameObject.layer == _enemyLayer && !_collidersDetectable.ContainsKey(colliderId))
            {
                if (collider.TryGetComponent(out IGuardStateDetectable detectable))
                {
                    _collidersDetectable.Add(colliderId, detectable);
                }
                else
                {
                    Debug.LogError("Collider doesn't have IGuardStateDetectable", collider);    
                }
            }
        }

        public void RemoveCollider(FieldOfViewEntity entity, Collider2D collider)
        {
            if (collider.isTrigger)
                return;

            if (!entity.isActiveAndEnabled)
            {
                return;
            }

            int entityId = entity.GetInstanceID();
            int colliderId = collider.GetInstanceID();

            if (!_entityes.ContainsKey(entityId))
            {
                Debug.LogError("Remove collider failed, entity isn't added", entity);
                return;
            }

            if (_entitiesColliders.TryGetFirstValue(entityId, out int currentColliderId,
                out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    if (currentColliderId == colliderId)
                    {
                        _entitiesColliders.Remove(iterator);
                        break;
                    }
                } while (_entitiesColliders.TryGetNextValue(out currentColliderId, ref iterator));
            }
            else
            {
                Debug.LogError("There is no entity to remove collider", entity);
                return;
            }

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

        public void AddEntity(FieldOfViewEntity entity)
        {
            int entityId = entity.GetInstanceID();
            if (!_entityes.TryAdd(entityId, entity))
            {
                Debug.LogError("Entity already added", entity);
                return;
            }

            if (_entitiesColliders.ContainsKey(entityId))
            {
                Debug.LogError("Found in _entitiesColliders", entity);
                return;
            }

            _wasEntitiesDicChanged = true;

            IncreaseCapasityOfEntitiesCollidersIfNeeded(10);
            _entitiesColliders.Add(entityId, _EMPTY_COLLIDER_ID);
        }

        public void RemoveEntity(FieldOfViewEntity entity)
        {
            int entityId = entity.GetInstanceID();

            if (!_entityes.ContainsKey(entityId))
            {
                Debug.LogError("Entity not found in _entityes", entity);
                return;
            }

            if (!_entitiesColliders.ContainsKey(entityId))
            {
                Debug.LogError("Entity not found in _entitiesColliders", entity);
            }

            if (_entitiesColliders.TryGetFirstValue(entityId, out int colliderId, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    if (_collidersToUnprepare.ContainsKey(colliderId))
                    {
                        var current = _collidersToUnprepare[colliderId];
                        if (current.Item2 <= 1)
                        {
                            _collidersToUnprepare.Remove(colliderId);
                        }
                        else
                        {
                            _collidersToUnprepare[colliderId] = (current.Item1, current.Item2 - 1);
                        }
                    }
                } while (_entitiesColliders.TryGetNextValue(out colliderId, ref iterator));
            }

            _entityes.Remove(entityId);
            _entitiesColliders.Remove(entityId);

            _wasEntitiesDicChanged = true;
        }

        public void OnEntityDataChange()
        {
            _wasEntitiesDicChanged = true;
        }

        public void DoWhenJobCompleted(Action action)
        {
            if(_isJobInProgress)
            {
                if(raycastsJobHandle.IsCompleted)
                {
                    CompliteUpdateView();
                    action?.Invoke();
                }
                else
                {
                    _onUpdateViewCompleted += action;
                }
            }
            else
            {
                action?.Invoke();
            }
        }

        private void ScheduleUpdateView()
        {
            _isJobInProgress = true;

            Profiler.BeginSample("amigus1-2 datasUnprep alloc");
            _datasUnprep.Clear();
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-3 vertsUnprep alloc");
            _vertsUnprep.Clear();
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-4 dataUnpare");
            foreach (var pair in _collidersToUnprepare)
            {
                Collider2D col = pair.Value.Item1;
                Transform colTrans = col.transform;
                switch (col)
                {
                    case BoxCollider2D box:
                        ColliderDataUnprepared boxData = new()
                        {
                            typeEnum = ColliderType.Box,
                            posWorld = (Vector2)colTrans.position,
                            rotWorld = colTrans.eulerAngles.z,
                            offsetLoc = box.offset,
                            lossyScale = (Vector2)colTrans.lossyScale,
                            sizeLoc = box.size,
                            colliderId = pair.Key,
                            layer = col.gameObject.layer
                        };
                        _datasUnprep.Add(boxData);
                        break;

                    case CircleCollider2D circle:
                        ColliderDataUnprepared circleData = new()
                        {
                            typeEnum = ColliderType.Circle,
                            posWorld = (Vector2)colTrans.position,
                            rotWorld = colTrans.eulerAngles.z,
                            offsetLoc = circle.offset,
                            lossyScale = (Vector2)colTrans.localScale,
                            radiusLoc = circle.radius,
                            colliderId = pair.Key,
                            layer = col.gameObject.layer
                        };
                        _datasUnprep.Add(circleData);
                        break;

                    case CapsuleCollider2D capsule:
                        ColliderDataUnprepared capsuleData = new()
                        {
                            typeEnum = ColliderType.Capsule,
                            offsetLoc = capsule.offset,
                            posWorld = (Vector2)colTrans.position,
                            rotWorld = colTrans.eulerAngles.z,
                            lossyScale = (Vector2)colTrans.lossyScale,
                            sizeLoc = capsule.size,
                            capsuleDirEnum = capsule.direction,
                            capsuleTransUpOrBoundsPos = (Vector2)colTrans.up,
                            capsuleTransRightOrBoundsSize = (Vector2)colTrans.right,
                            colliderId = pair.Key,
                            layer = col.gameObject.layer
                        };
                        _datasUnprep.Add(capsuleData);
                        break;

                    case PolygonCollider2D poly:
                        Vector2[] points = poly.points;
                        //Profiler.EndSample();

                        //Profiler.BeginSample("amigus polygon 2");
                        Bounds boundsPoly = col.bounds;
                        ColliderDataUnprepared polyData = new()
                        {
                            typeEnum = ColliderType.Polygon,
                            vertexStartIndex = _vertsUnprep.Length,
                            posWorld = (Vector2)colTrans.position,
                            rotWorld = colTrans.eulerAngles.z,
                            lossyScale = (Vector2)colTrans.lossyScale,
                            vertexCount = points.Length,
                            capsuleTransUpOrBoundsPos = (Vector2)boundsPoly.center,
                            capsuleTransRightOrBoundsSize = (Vector2)boundsPoly.size,
                            colliderId = pair.Key,
                            layer = col.gameObject.layer
                        };
                        //Profiler.EndSample();

                        //Profiler.BeginSample("amigus polygon 3");
                        unsafe
                        {
                            fixed (Vector2* ptr = points)
                            {
                                _vertsUnprep.AddRange(ptr, points.Length);
                            }
                        }

                        //Profiler.EndSample();
                        //Profiler.BeginSample("amigus polygon 4");
                        _datasUnprep.Add(polyData);
                        break;

                    case EdgeCollider2D edge:
                        Vector2[] edgePoints = edge.points;
                        Bounds boundsEdge = col.bounds;
                        ColliderDataUnprepared edgeData = new()
                        {
                            typeEnum = ColliderType.Edge,
                            vertexStartIndex = _vertsUnprep.Length,
                            posWorld = (Vector2)colTrans.position,
                            rotWorld = colTrans.eulerAngles.z,
                            lossyScale = (Vector2)colTrans.lossyScale,
                            vertexCount = edgePoints.Length,
                            capsuleTransUpOrBoundsPos = (Vector2)boundsEdge.center,
                            capsuleTransRightOrBoundsSize = (Vector2)boundsEdge.size,
                            colliderId = pair.Key,
                            layer = col.gameObject.layer
                        };

                        unsafe
                        {
                            fixed (Vector2* ptr = edgePoints)
                            {
                                _vertsUnprep.AddRange(ptr, edgePoints.Length);
                            }
                        }

                        _datasUnprep.Add(edgeData);
                        break;

                    case CompositeCollider2D composite:
                        for (int p = 0; p < composite.pathCount; p++)
                        {
                            //Profiler.BeginSample("amigus composite 1");
                            int pointCount = composite.GetPathPointCount(p);
                            //Profiler.EndSample();

                            //Profiler.BeginSample("amigus composite 2");
                            Bounds boundsComposite = col.bounds;
                            ColliderDataUnprepared compositeData = new()
                            {
                                typeEnum = ColliderType.Composite,
                                vertexStartIndex = _vertsUnprep.Length,
                                posWorld = (Vector2)colTrans.position,
                                rotWorld = colTrans.eulerAngles.z,
                                vertexCount = pointCount,
                                capsuleTransUpOrBoundsPos = (Vector2)boundsComposite.center,
                                capsuleTransRightOrBoundsSize = (Vector2)boundsComposite.size,
                                colliderId = pair.Key,
                                layer = col.gameObject.layer
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
                                    _vertsUnprep.AddRange(pathPointsPtr, pointCount);
                                }
                            }

                            //Profiler.EndSample();

                            //Profiler.BeginSample("amigus composite 4");
                            _datasUnprep.Add(compositeData);
                            //Profiler.EndSample();
                        }
                        break;

                    default:
                        ColliderDataUnprepared defaultData = new()
                        {
                            typeEnum = ColliderType.Unsuported,
                            posWorld = (Vector2)col.bounds.center,
                            sizeLoc = (Vector2)col.bounds.size,
                            colliderId = pair.Key,
                            layer = col.gameObject.layer
                        };
                        _datasUnprep.Add(defaultData);
                        Debug.LogError("Unsuported collider type");
                        break;

                }
            }
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-5 datasRdy alloc");
            // int - colliderId
            if (_datasRdy.Capacity < _datasUnprep.Length)
            {
                _datasRdy.Dispose();
                _datasRdy = new(_datasUnprep.Length + 10, Allocator.Persistent);
            }
            else
            {
                _datasRdy.Clear();
            }
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-6 vertsRdy alloc");
            _vertsRdy.Resize(_vertsUnprep.Length, NativeArrayOptions.UninitializedMemory);
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-7-1 dataPrepare create job");
            PrepareColliderDatasJob prepareJob = new PrepareColliderDatasJob
            {
                datasUnprep = _datasUnprep,
                vertsUnprep = _vertsUnprep,
                datasRdy = _datasRdy,
                vertsRdy = _vertsRdy,
            };
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-7-2 dataPrepare run job");
            prepareJob.Run();
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-8-2 rayJob fovDatas alloc");
            // int - entityId
            if (_fovDatas.Capacity < _entityes.Count)
            {
                _fovDatas.Dispose();
                _fovDatas = new(_datasUnprep.Length + 10, Allocator.Persistent);
            }
            else
            {
                _fovDatas.Clear();
            }
            verticiesCount = 0;
            int rayCount = 0;
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-8-3 rayJob fovDatas create");
            float currentMeshMoveZ = 0;
            foreach (var entity in _entityes)
            {
                _fovDatas.Add(entity.Key, entity.Value.GetData(rayCount, verticiesCount,
                    currentMeshMoveZ));
                verticiesCount += _fovDatas[entity.Key].rayCount + 2;
                rayCount += _fovDatas[entity.Key].rayCount;
                currentMeshMoveZ += _meshMoveZStep;
            }
            Profiler.EndSample();

            Profiler.BeginSample("amigus1-9 rayJob verticies array");
            if (_verticies.Length < verticiesCount)
            {
                _verticies.Dispose();
                _verticies = new(verticiesCount, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory);
            }
            Profiler.EndSample();

            Profiler.BeginSample("amigus2-2-1 rayJob triangles array");
            trianglesCount = rayCount * 3;
            if (_triangles.Length < trianglesCount)
            {
                _triangles.Dispose();
                _triangles = new NativeArray<int>(trianglesCount, Allocator.Persistent,
                    NativeArrayOptions.ClearMemory);
            }
            Profiler.EndSample();

            Profiler.BeginSample("amigus2-2-2 rayJob enemiesPlayerHit alloc");
            if (_enemiesPlayerHit.Capacity < _entityes.Count)
            {
                _enemiesPlayerHit.Dispose();
                _enemiesPlayerHit = new(_entityes.Count, Allocator.Persistent);
            }
            else
            {
                _enemiesPlayerHit.Clear();
            }
            Profiler.EndSample();

            Profiler.BeginSample("amigus2-2-3 rayJob enemiesEnemyHit alloc");
            int newCap = (_entityes.Count - 1) * (_entityes.Count - 1) + 10  /** _entityes.Count * 2 + 10*/;
            if (_enemiesEnemyHit.Capacity < newCap)
            {
                _enemiesEnemyHit.Dispose();
                _enemiesEnemyHit = new(newCap + 10, Allocator.Persistent);
            }
            else
            {
                _enemiesEnemyHit.Clear();
            }
            Profiler.EndSample();

            Profiler.BeginSample("amigus2-3 rayJob create");
            Raycast2DWithMeshJob raycastsJob = new Raycast2DWithMeshJob
            {
                fovEntityDatas = _fovDatas,
                entitiesColliders = _entitiesColliders,
                colliderDataArray = _datasRdy,
                vertexArray = _vertsRdy,
                verticies = _verticies,
                triangles = _triangles,
                enemiesPlayerHit = _enemiesPlayerHit.AsParallelWriter(),
                enemiesEnemyHit = _enemiesEnemyHit.AsParallelWriter(),
                playerLayer = LayerMask.NameToLayer(Layers.Player),
                enemyLayer = LayerMask.NameToLayer(Layers.Enemy),
            };
            Profiler.EndSample();

            Profiler.BeginSample("amigus2-4-1 rayJob Schedule");
            int batchCount = CalculateOptimalBatchSize(verticiesCount);
            raycastsJobHandle = raycastsJob.Schedule(verticiesCount, batchCount);
            //JobHandle raycastsJobHandle = raycastsJob.Schedule(verticiesCount, new JobHandle());
            //raycastsJobHandle.Complete();
            Profiler.EndSample();
        }

        JobHandle raycastsJobHandle;
        int verticiesCount;
        int trianglesCount;

        bool _isJobInProgress = false;

        private void CompliteUpdateView()
        {
            if(!_isJobInProgress)
                return;

            Profiler.BeginSample("amigus2-4-2 rayJob Complite");
            raycastsJobHandle.Complete();
            Profiler.EndSample();

            Profiler.BeginSample("amigus2-6 mesh vertices");
            if (_wasEntitiesDicChanged)
            {
                _mesh.SetVertexBufferParams(verticiesCount, _vertexAttributeDescriptor);
            }
            _mesh.SetVertexBufferData(_verticies, 0, 0, verticiesCount, 0,
                MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices);
            Profiler.EndSample();

            if (_wasEntitiesDicChanged)
            {
                Profiler.BeginSample("amigus2-5 mesh triangles");
                _mesh.SetIndices(_triangles, 0, trianglesCount, MeshTopology.Triangles, 0, false, 0);
                Profiler.EndSample();
            }
            _wasEntitiesDicChanged = false;

            Profiler.BeginSample("amigus2-7-1 mesh RecalculateBounds");
            _mesh.RecalculateBounds(MeshUpdateFlags.DontRecalculateBounds |
                    MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices);
            Profiler.EndSample();

            Profiler.BeginSample("amigus2-7-2 Found player events");
            foreach (var found in _enemiesPlayerHit)
            {
                _entityes[found.Key].OnPlayerFound();
            }
            Profiler.EndSample();

            Profiler.BeginSample("amigus2-7-3 Found enemy events");
            foreach (var found in _enemiesEnemyHit)
            {
                if (!_collidersDetectable[found.Key.hitEnemyColliderId].IsEnemyInGuardState)
                {
                    _entityes[found.Key.rayCasterEnemyId].OnEnemyNotInGuardStateFound();
                }
            }
            Profiler.EndSample();

            Profiler.BeginSample("amigus2-8 OnUpdateViewCompleted Invoke");
            _onUpdateViewCompleted?.Invoke();
            _onUpdateViewCompleted = null;
            Profiler.EndSample();
            _isJobInProgress = false;
        }

        private void IncreaseCapasityOfEntitiesCollidersIfNeeded(int capIncrease)
        {
            if (_entitiesColliders.Capacity >= _entityes.Count + 1)
            {
                return;
            }

            int newCapacity = _entitiesColliders.Capacity + capIncrease;
            NativeMultiHashMap<int, int> newMap = new(newCapacity, Allocator.Persistent);

            NativeMultiHashMapIterator<int> iterator;
            int value;

            var nonUniqueKeys = _entitiesColliders.GetKeyArray(Allocator.Temp);
            NativeHashSet<int> uniqueKeys = new(nonUniqueKeys.Length, Allocator.Temp);
            foreach (var key in nonUniqueKeys)
            {
                if (uniqueKeys.Contains(key))
                    continue;

                uniqueKeys.Add(key);
            }
            nonUniqueKeys.Dispose();

            foreach (var key in uniqueKeys)
            {
                if (_entitiesColliders.TryGetFirstValue(key, out value, out iterator))
                {
                    do
                    {
                        newMap.Add(key, value);
                    }
                    while (_entitiesColliders.TryGetNextValue(out value, ref iterator));
                }
            }

            uniqueKeys.Dispose();

            // Dispose the old map and assign the new one.
            _entitiesColliders.Dispose();
            _entitiesColliders = newMap;
        }

        //To avoid StateMachine Enabel/Disable at start
        private IEnumerator SubscribeCustomLoop()
        {
            yield return new WaitForEndOfFrame();
            CustomPlayerLoopInjection.OnAfterPhysics2DUpdate += ScheduleUpdateView;
            CustomPlayerLoopInjection.OnPostLateUpdateEnd += CompliteUpdateView;
        }

        private int CalculateOptimalBatchSize(int arrayLength)
        {
            int targetThreads = Mathf.Max(1, (JobsUtility.JobWorkerCount * 3) - 4);

            int batchSize = Mathf.CeilToInt((float)arrayLength / targetThreads);
            return Mathf.Max(1, batchSize);
        }
    }
}
