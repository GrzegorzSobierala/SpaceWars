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
        private int _verticiesCount;
        private int _trianglesCount;

        private FieldOfViewSystemCollectionsCache _collections;
        private FieldOfViewSystemFacade _facade;

        public const int EMPTY_COLLIDER_ID = 0;

        public FieldOfViewSystemFacade Facade => _facade;

        private void Awake()
        {
            InitLayerFilters();
            InitMesh();
            InitSystemComponents();
        }

        private void Start()
        {
            //To avoid EnemiyStateMachine Enabel/Disable at start
            StartCoroutine(SubscribeToCustomLoop());
        }

        private void OnDestroy()
        {
            UnsubscribeToCustomLoop();
        }

        private void InitLayerFilters()
        {
            _allLayerMask = LayerMask.GetMask(Layers.Player, Layers.Obstacle, Layers.Enemy);
            _contactFilter = new ContactFilter2D
            {
                useTriggers = false,
                layerMask = _allLayerMask,
                useLayerMask = true
            };
            _enemyLayer = LayerMask.NameToLayer(Layers.Enemy);
        }

        private void InitMesh()
        {
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
        }

        private void InitSystemComponents()
        {
            _collections = new FieldOfViewSystemCollectionsCache();

            Action<Action> doWhenJobComleted = (action) => DoWhenJobCompleted(action);
            Func<int> getEnemyLayer = () => _enemyLayer;
            _facade = new FieldOfViewSystemFacade(_collections, doWhenJobComleted, getEnemyLayer);
        }

        private void UnsubscribeToCustomLoop()
        {
            CustomPlayerLoopInjection.OnAfterPhysics2DUpdate -= ScheduleUpdateView;
            CustomPlayerLoopInjection.OnPostLateUpdateEnd -= CompliteUpdateView;
        }

        private void ScheduleUpdateView()
        {
            _isJobInProgress = true;

            _collections.datasUnprep.Clear();
            _collections.vertsUnprep.Clear();
            FillDatasUnprepWithCurrentCollidersData();

            if (_collections.datasRdy.Capacity < _collections.datasUnprep.Length)
            {
                _collections.datasRdy.Dispose();
                _collections.datasRdy = new(_collections.datasUnprep.Length + 10,
                    Allocator.Persistent);
            }
            else
            {
                _collections.datasRdy.Clear();
            }

            _collections.vertsRdy.Resize(_collections.vertsUnprep.Length, 
                NativeArrayOptions.UninitializedMemory);

            PrepareColliderDatasJob prepareJob = new PrepareColliderDatasJob
            {
                datasUnprep = _collections.datasUnprep,
                vertsUnprep = _collections.vertsUnprep,
                datasRdy = _collections.datasRdy,
                vertsRdy = _collections.vertsRdy,
            };

            prepareJob.Run();

            if (_collections.fovDatas.Capacity < _collections.entities.Count)
            {
                _collections.fovDatas.Dispose();
                _collections.fovDatas = new(_collections.datasUnprep.Length + 10,
                    Allocator.Persistent);
            }
            else
            {
                _collections.fovDatas.Clear();
            }
            _verticiesCount = 0;
            int rayCount = 0;

            float currentMeshMoveZ = 0;
            foreach (var entity in _collections.entities)
            {
                _collections.fovDatas.Add(entity.Key, entity.Value.GetData(rayCount, _verticiesCount,
                    currentMeshMoveZ));
                _verticiesCount += _collections.fovDatas[entity.Key].rayCount + 2;
                rayCount += _collections.fovDatas[entity.Key].rayCount;
                currentMeshMoveZ += _meshMoveZStep;
            }

            if (_collections.verticies.Length < _verticiesCount)
            {
                _collections.verticies.Dispose();
                _collections.verticies = new(_verticiesCount, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory);
            }

            _trianglesCount = rayCount * 3;
            if (_collections.triangles.Length < _trianglesCount)
            {
                _collections.triangles.Dispose();
                _collections.triangles = new NativeArray<int>(_trianglesCount, Allocator.Persistent,
                    NativeArrayOptions.ClearMemory);
            }

            if (_collections.enemiesPlayerHit.Capacity < _collections.entities.Count)
            {
                _collections.enemiesPlayerHit.Dispose();
                _collections.enemiesPlayerHit = new(_collections.entities.Count, Allocator.Persistent);
            }
            else
            {
                _collections.enemiesPlayerHit.Clear();
            }

            int newCap = (_collections.entities.Count - 1) * (_collections.entities.Count - 1) + 10  /** _entityes.Count * 2 + 10*/;
            if (_collections.enemiesEnemyHit.Capacity < newCap)
            {
                _collections.enemiesEnemyHit.Dispose();
                _collections.enemiesEnemyHit = new(newCap + 10, Allocator.Persistent);
            }
            else
            {
                _collections.enemiesEnemyHit.Clear();
            }

            Raycast2DWithMeshJob raycastsJob = new Raycast2DWithMeshJob
            {
                fovEntityDatas = _collections.fovDatas,
                entitiesColliders = _collections.entitiesColliders,
                colliderDataArray = _collections.datasRdy,
                vertexArray = _collections.vertsRdy,
                verticies = _collections.verticies,
                triangles = _collections.triangles,
                enemiesPlayerHit = _collections.enemiesPlayerHit.AsParallelWriter(),
                enemiesEnemyHit = _collections.enemiesEnemyHit.AsParallelWriter(),
                playerLayer = LayerMask.NameToLayer(Layers.Player),
                enemyLayer = LayerMask.NameToLayer(Layers.Enemy),
            };

            int batchCount = CalculateOptimalBatchSize(_verticiesCount);
            raycastsJobHandle = raycastsJob.Schedule(_verticiesCount, batchCount);
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
                _mesh.SetVertexBufferParams(_verticiesCount, _vertexAttributeDescriptor);
            }
            _mesh.SetVertexBufferData(_collections.verticies, 0, 0, _verticiesCount, 0,
                MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices);

            if (_wasEntitiesDicChanged)
            {
                _mesh.SetIndices(_collections.triangles, 0, _trianglesCount, MeshTopology.Triangles,
                    0, false, 0);
            }
            _wasEntitiesDicChanged = false;

            _mesh.RecalculateBounds(MeshUpdateFlags.DontRecalculateBounds |
                    MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices);

            foreach (var found in _collections.enemiesPlayerHit)
            {
                _collections.entities[found.Key].OnPlayerFound();
            }

            foreach (var found in _collections.enemiesEnemyHit)
            {
                IGuardStateDetectable detectable = _collections.collidersDetectable[found.Key.
                    hitEnemyColliderId];
                
                if(_collections.entities.ContainsKey(found.Key.rayCasterEnemyId))
                {
                    _collections.entitiesController[found.Key.rayCasterEnemyId].
                        OnEnemySeeEnemy(detectable);

                    if (!detectable.IsEnemyInGuardState)
                    {
                        _collections.entities[found.Key.rayCasterEnemyId].OnEnemyNotInGuardStateFound();
                    }
                }
            }

            foreach (var controller in _collections.controllers)
            {
                controller.OnPostEnemySeeEnemy();
            }

            _onUpdateViewCompleted?.Invoke();
            _onUpdateViewCompleted = null;
            _isJobInProgress = false;
        }

        private void FillDatasUnprepWithCurrentCollidersData()
        {
            foreach (var pair in _collections.collidersUnprepared)
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
                        _collections.datasUnprep.Add(boxData);
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
                        _collections.datasUnprep.Add(circleData);
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
                        _collections.datasUnprep.Add(capsuleData);
                        break;

                    case PolygonCollider2D poly:
                        Vector2[] points = poly.points;

                        Bounds boundsPoly = col.bounds;
                        ColliderDataUnprepared polyData = new()
                        {
                            typeEnum = ColliderType.Polygon,
                            vertexStartIndex = _collections.vertsUnprep.Length,
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
                                _collections.vertsUnprep.AddRange(ptr, points.Length);
                            }
                        }

                        _collections.datasUnprep.Add(polyData);
                        break;

                    case EdgeCollider2D edge:
                        Vector2[] edgePoints = edge.points;
                        Bounds boundsEdge = col.bounds;
                        ColliderDataUnprepared edgeData = new()
                        {
                            typeEnum = ColliderType.Edge,
                            vertexStartIndex = _collections.vertsUnprep.Length,
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
                                _collections.vertsUnprep.AddRange(ptr, edgePoints.Length);
                            }
                        }

                        _collections.datasUnprep.Add(edgeData);
                        break;

                    case CompositeCollider2D composite:
                        for (int p = 0; p < composite.pathCount; p++)
                        {
                            int pointCount = composite.GetPathPointCount(p);

                            Bounds boundsComposite = col.bounds;
                            ColliderDataUnprepared compositeData = new()
                            {
                                typeEnum = ColliderType.Composite,
                                vertexStartIndex = _collections.vertsUnprep.Length,
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
                                if (_collections.pathPointsCompositeCache.Length < pointCount)
                                {
                                    _collections.pathPointsCompositeCache = new Vector2[pointCount];
                                }
                                composite.GetPath(p, _collections.pathPointsCompositeCache);
                                fixed (Vector2* pathPointsPtr = _collections.pathPointsCompositeCache)
                                {
                                    _collections.vertsUnprep.AddRange(pathPointsPtr, pointCount);
                                }
                            }

                            _collections.datasUnprep.Add(compositeData);
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
                        _collections.datasUnprep.Add(defaultData);
                        Debug.LogError("Unsuported collider type");
                        break;
                }
            }
        }

        private void DoWhenJobCompleted(Action action)
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
