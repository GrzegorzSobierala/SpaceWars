using Game.Utility.Globals;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using Game.Room.Enemy;
using System.Collections;

namespace Game.Physics
{
    [DefaultExecutionOrder(-100)]
    public class FieldOfViewSystem : MonoBehaviour
    {
        [SerializeField] private float _meshMoveZStep = 0.001f;

        private event Action _onUpdateViewCompleted;

        private MeshFilter _meshFilter;
        private Mesh _mesh;
        private VertexAttributeDescriptor _vertexAttributeDescriptor;
        private LayerMask _allLayerMask;
        private ContactFilter2D _contactFilter;
        private int _enemyLayer;

        private bool _wasEntitiesDicChanged = false;

        private bool _isJobInProgress = false;
        private JobHandle raycastsJobHandle;
        private int verticiesCount;
        private int trianglesCount;

        // int1 - ColliderId, int2 - enter count
        private Dictionary<int, (Collider2D, int)> _collidersUnprepared = new();
        // int - EntityId
        private Dictionary<int, FieldOfViewEntity> _entityes = new();
        // int - enemy's colliderId
        private Dictionary<int, IGuardStateDetectable> _collidersDetectable = new();
        //int - EntityId
        private Dictionary<int, FieldOfViewEntitiesController> _entitiesController = new();
        private HashSet<FieldOfViewEntitiesController> _controllers = new();

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

        public const int EMPTY_COLLIDER_ID = 0;

        public LayerMask AllLayerMask => _allLayerMask;
        public ContactFilter2D ContactFilter => _contactFilter;

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
            StartCoroutine(SubscribeToCustomLoop());
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

            if (_collidersUnprepared.ContainsKey(colliderId))
            {
                var current = _collidersUnprepared[colliderId];
                _collidersUnprepared[colliderId] = (collider, current.Item2 + 1);
            }
            else
            {
                _collidersUnprepared.Add(colliderId, (collider, 1));
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

            if (_collidersUnprepared.ContainsKey(colliderId))
            {
                if (_collidersUnprepared[colliderId].Item2 <= 1)
                {
                    if (_collidersUnprepared[colliderId].Item2 <= 0)
                    {
                        Debug.LogError("_collidersToUnprepare[colliderId].Item2 <= 0 - entity ref", entity);
                        Debug.LogError("_collidersToUnprepare[colliderId].Item2 <= 0 - collider ref", collider);
                    }

                    _collidersUnprepared.Remove(colliderId);
                }
                else
                {
                    var current = _collidersUnprepared[colliderId];
                    _collidersUnprepared[colliderId] = (collider, current.Item2 - 1);
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
                //Debug.LogError("Entity already added", entity);
                return;
            }

            if (_entitiesColliders.ContainsKey(entityId))
            {
                Debug.LogError("Found in _entitiesColliders", entity);
                return;
            }

            _wasEntitiesDicChanged = true;

            IncreaseCapasityOfEntitiesCollidersIfNeeded(10);
            _entitiesColliders.Add(entityId, EMPTY_COLLIDER_ID);
        }

        public void RemoveEntity(FieldOfViewEntity entity)
        {
            int entityId = entity.GetInstanceID();

            if (!_entityes.ContainsKey(entityId))
            {
                //Debug.LogError("Entity not found in _entityes", entity);
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
                    if (_collidersUnprepared.ContainsKey(colliderId))
                    {
                        var current = _collidersUnprepared[colliderId];
                        if (current.Item2 <= 1)
                        {
                            _collidersUnprepared.Remove(colliderId);
                        }
                        else
                        {
                            _collidersUnprepared[colliderId] = (current.Item1, current.Item2 - 1);
                        }
                    }
                } while (_entitiesColliders.TryGetNextValue(out colliderId, ref iterator));
            }

            _entityes.Remove(entityId);
            _entitiesColliders.Remove(entityId);

            _wasEntitiesDicChanged = true;
        }

        public void AddController(FieldOfViewEntitiesController controller, FieldOfViewEntity entity)
        {
            if (!_entitiesController.ContainsValue(controller))
            {
                _controllers.Add(controller);
            }

            _entitiesController.Add(entity.GetInstanceID(), controller);
        }

        public void RemoveController(FieldOfViewEntitiesController controller, FieldOfViewEntity entity)
        {
            _entitiesController.Remove(entity.GetInstanceID());

            if (!_entitiesController.ContainsValue(controller))
            {
                _controllers.Remove(controller);
            }
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

            _datasUnprep.Clear();
            _vertsUnprep.Clear();
            FillDatasUnprepWithCurrentCollidersData();

            if (_datasRdy.Capacity < _datasUnprep.Length)
            {
                _datasRdy.Dispose();
                _datasRdy = new(_datasUnprep.Length + 10, Allocator.Persistent);
            }
            else
            {
                _datasRdy.Clear();
            }

            _vertsRdy.Resize(_vertsUnprep.Length, NativeArrayOptions.UninitializedMemory);

            PrepareColliderDatasJob prepareJob = new PrepareColliderDatasJob
            {
                datasUnprep = _datasUnprep,
                vertsUnprep = _vertsUnprep,
                datasRdy = _datasRdy,
                vertsRdy = _vertsRdy,
            };

            prepareJob.Run();

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

            float currentMeshMoveZ = 0;
            foreach (var entity in _entityes)
            {
                _fovDatas.Add(entity.Key, entity.Value.GetData(rayCount, verticiesCount,
                    currentMeshMoveZ));
                verticiesCount += _fovDatas[entity.Key].rayCount + 2;
                rayCount += _fovDatas[entity.Key].rayCount;
                currentMeshMoveZ += _meshMoveZStep;
            }

            if (_verticies.Length < verticiesCount)
            {
                _verticies.Dispose();
                _verticies = new(verticiesCount, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory);
            }

            trianglesCount = rayCount * 3;
            if (_triangles.Length < trianglesCount)
            {
                _triangles.Dispose();
                _triangles = new NativeArray<int>(trianglesCount, Allocator.Persistent,
                    NativeArrayOptions.ClearMemory);
            }

            if (_enemiesPlayerHit.Capacity < _entityes.Count)
            {
                _enemiesPlayerHit.Dispose();
                _enemiesPlayerHit = new(_entityes.Count, Allocator.Persistent);
            }
            else
            {
                _enemiesPlayerHit.Clear();
            }

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

            int batchCount = CalculateOptimalBatchSize(verticiesCount);
            raycastsJobHandle = raycastsJob.Schedule(verticiesCount, batchCount);
            //Version to Debug on main thread
            //JobHandle raycastsJobHandle = raycastsJob.Schedule(verticiesCount, new JobHandle());
            //raycastsJobHandle.Complete();
        }
        
        private void CompliteUpdateView()
        {
            if(!_isJobInProgress)
                return;

            raycastsJobHandle.Complete();

            if (_wasEntitiesDicChanged)
            {
                _mesh.SetVertexBufferParams(verticiesCount, _vertexAttributeDescriptor);
            }
            _mesh.SetVertexBufferData(_verticies, 0, 0, verticiesCount, 0,
                MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices);

            if (_wasEntitiesDicChanged)
            {
                _mesh.SetIndices(_triangles, 0, trianglesCount, MeshTopology.Triangles, 0, false, 0);
            }
            _wasEntitiesDicChanged = false;

            _mesh.RecalculateBounds(MeshUpdateFlags.DontRecalculateBounds |
                    MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices);

            foreach (var found in _enemiesPlayerHit)
            {
                _entityes[found.Key].OnPlayerFound();
            }

            foreach (var found in _enemiesEnemyHit)
            {
                IGuardStateDetectable detectable = _collidersDetectable[found.Key.hitEnemyColliderId];
                
                if(_entityes.ContainsKey(found.Key.rayCasterEnemyId))
                {
                    _entitiesController[found.Key.rayCasterEnemyId].OnEnemySeeEnemy(detectable);

                    if (!detectable.IsEnemyInGuardState)
                    {
                        _entityes[found.Key.rayCasterEnemyId].OnEnemyNotInGuardStateFound(detectable);
                    }
                }


                //if (!_entityes.ContainsKey(found.Key.rayCasterEnemyId))
                //    return;

                //_entitiesController[found.Key.rayCasterEnemyId].OnEnemySeeEnemy(detectable);

                //if (detectable.IsEnemyInGuardState)
                //    return;

                //_entityes[found.Key.rayCasterEnemyId].OnEnemyNotInGuardStateFound(detectable);
            }

            foreach (var controller in _controllers)
            {
                controller.OnPostEnemySeeEnemy();
            }

            _onUpdateViewCompleted?.Invoke();
            _onUpdateViewCompleted = null;
            _isJobInProgress = false;
        }

        private void FillDatasUnprepWithCurrentCollidersData()
        {
            foreach (var pair in _collidersUnprepared)
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

                        unsafe
                        {
                            fixed (Vector2* ptr = points)
                            {
                                _vertsUnprep.AddRange(ptr, points.Length);
                            }
                        }

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
                            int pointCount = composite.GetPathPointCount(p);

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

                            _datasUnprep.Add(compositeData);
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
        private IEnumerator SubscribeToCustomLoop()
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
