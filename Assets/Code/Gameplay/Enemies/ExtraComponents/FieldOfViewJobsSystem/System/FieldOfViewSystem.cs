using Game.Utility.Globals;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using Game.Room.Enemy;

namespace Game.Physics
{
    [DefaultExecutionOrder(-100)]
    public class FieldOfViewSystem : MonoBehaviour
    {
        private event Action OnUpdateViewCompleted;

        [SerializeField] private float _meshMoveZStep = 0.001f;

        private MeshFilter _meshFilter;
        private Mesh _mesh;
        private VertexAttributeDescriptor _vertexAttributeDescriptor;
        private int _enemyLayer;

        private bool _isJobInProgress = false;
        private JobHandle _raycastsJobHandle;
        private int _verticiesCount;
        private int _trianglesCount;

        private FieldOfViewSystemCollectionsCache _collections;
        private FieldOfViewSystemFacade _facade;

        public const int EMPTY_COLLIDER_ID = 0;

        public FieldOfViewSystemFacade Facade => _facade;

        private void Awake()
        {
            _enemyLayer = LayerMask.NameToLayer(Layers.Enemy);
            InitMesh();
            InitSystemComponents();
        }

        private void Start()
        {
            SubscribeToCustomLoop();
        }

        private void OnDestroy()
        {
            UnsubscribeToCustomLoop();
            _collections.DisposeCollections();
        }

        #region InitializeSystem

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
            _collections.InitCollections();

            void doWhenJobComleted(Action action) => DoWhenJobCompleted(action);
            int getEnemyLayer() => _enemyLayer;
            _facade = new FieldOfViewSystemFacade(_collections, doWhenJobComleted, getEnemyLayer);
        }
        #endregion

        #region ScheduleWork
        private void ScheduleUpdateView()
        {
            _isJobInProgress = true;

            ColliderDataConverter.UpdateColliderUnprepareDatas(_collections);

            PrepareDatasReadyCollection();
            _collections.vertsRdy.Resize(_collections.vertsUnprep.Length,
                NativeArrayOptions.UninitializedMemory);

            RunPrepareColliderDatasJob();

            PrepareFovDatasCollection();
            FillFovDatasCollection(out int rayCount);

            PrepareMeshCollections(rayCount);
            PrepareEnemiesPlayerHitCollection();
            PrepareEnemiesEnemyHitCollection();

            ScheduleRaycast2DWithMeshJob();
        }

        private void PrepareDatasReadyCollection()
        {
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
        }

        private void RunPrepareColliderDatasJob()
        {
            PrepareColliderDatasJob prepareJob = new()
            {
                datasUnprep = _collections.datasUnprep,
                vertsUnprep = _collections.vertsUnprep,
                datasRdy = _collections.datasRdy,
                vertsRdy = _collections.vertsRdy,
            };

            prepareJob.Run();
        }

        private void PrepareFovDatasCollection()
        {
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
        }

        private void FillFovDatasCollection(out int rayCount)
        {
            rayCount = 0;
            _verticiesCount = 0;

            float currentMeshMoveZ = 0;
            foreach (var entity in _collections.entities)
            {
                _collections.fovDatas.Add(entity.Key, entity.Value.GetData(rayCount, _verticiesCount,
                    currentMeshMoveZ));
                _verticiesCount += _collections.fovDatas[entity.Key].rayCount + 2;
                rayCount += _collections.fovDatas[entity.Key].rayCount;
                currentMeshMoveZ += _meshMoveZStep;
            }
        }

        private void PrepareMeshCollections(int rayCount)
        {
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
                _collections.triangles = new(_trianglesCount, Allocator.Persistent,
                    NativeArrayOptions.ClearMemory);
            }
        }

        private void PrepareEnemiesPlayerHitCollection()
        {
            if (_collections.enemiesPlayerHit.Capacity < _collections.entities.Count)
            {
                _collections.enemiesPlayerHit.Dispose();
                _collections.enemiesPlayerHit = new(_collections.entities.Count, Allocator.Persistent);
            }
            else
            {
                _collections.enemiesPlayerHit.Clear();
            }
        }

        private void PrepareEnemiesEnemyHitCollection()
        {
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
        }

        private void ScheduleRaycast2DWithMeshJob()
        {
            CreateEnemiesFieldOfViewJob raycastsJob = new()
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
            _raycastsJobHandle = raycastsJob.Schedule(_verticiesCount, batchCount);
            //Version to Debug on main thread
            //JobHandle raycastsJobHandle = raycastsJob.Schedule(verticiesCount, new JobHandle());
            //raycastsJobHandle.Complete();
        }
        #endregion

        #region CompliteWork
        private void CompliteUpdateView()
        {
            if (!_isJobInProgress)
                return;

            _raycastsJobHandle.Complete();

            ApplayNewShapeToMesh();

            NotifyEnemiesWhoFoundPlayer();
            NotifyEnemiesWhoFoundEnemies();

            foreach (var controller in _collections.controllers)
            {
                controller.OnPostEnemySeeEnemy();
            }

            OnUpdateViewCompleted?.Invoke();
            OnUpdateViewCompleted = null;
            _isJobInProgress = false;
        }

        private void ApplayNewShapeToMesh()
        {
            if (_collections.wasEntitiesDicChanged)
            {
                _mesh.SetVertexBufferParams(_verticiesCount, _vertexAttributeDescriptor);
            }
            _mesh.SetVertexBufferData(_collections.verticies, 0, 0, _verticiesCount, 0,
                MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices);

            if (_collections.wasEntitiesDicChanged)
            {
                _mesh.SetIndices(_collections.triangles, 0, _trianglesCount, MeshTopology.Triangles,
                    0, false, 0);
            }
            _collections.wasEntitiesDicChanged = false;

            _mesh.RecalculateBounds(MeshUpdateFlags.DontRecalculateBounds |
                    MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices);
        }

        private void NotifyEnemiesWhoFoundPlayer()
        {
            foreach (var found in _collections.enemiesPlayerHit)
            {
                _collections.entities[found.Key].OnPlayerFound();
            }
        }

        private void NotifyEnemiesWhoFoundEnemies()
        {
            foreach (var found in _collections.enemiesEnemyHit)
            {
                IGuardStateDetectable detectable = _collections.collidersDetectable[found.Key.
                    hitEnemyColliderId];

                if (_collections.entities.ContainsKey(found.Key.rayCasterEnemyId))
                {
                    _collections.entitiesController[found.Key.rayCasterEnemyId].
                        OnEnemySeeEnemy(detectable);

                    if (!detectable.IsEnemyInGuardState)
                    {
                        _collections.entities[found.Key.rayCasterEnemyId].OnEnemyNotInGuardStateFound();
                    }
                }
            }
        }
        #endregion

        private void DoWhenJobCompleted(Action action)
        {
            if(_isJobInProgress)
            {
                if(_raycastsJobHandle.IsCompleted)
                {
                    CompliteUpdateView();
                    action?.Invoke();
                }
                else
                {
                    OnUpdateViewCompleted += action;
                }
            }
            else
            {
                action?.Invoke();
            }
        }

        private int CalculateOptimalBatchSize(int arrayLength)
        {
            int targetThreads = Mathf.Max(1, (JobsUtility.JobWorkerCount * 3) - 4);

            int batchSize = Mathf.CeilToInt((float)arrayLength / targetThreads);
            return Mathf.Max(1, batchSize);
        }

        private void SubscribeToCustomLoop()
        {
            CustomPlayerLoopInjection.OnAfterPhysics2DUpdate += ScheduleUpdateView;
            CustomPlayerLoopInjection.OnPostLateUpdateEnd += CompliteUpdateView;
        }

        private void UnsubscribeToCustomLoop()
        {
            CustomPlayerLoopInjection.OnAfterPhysics2DUpdate -= ScheduleUpdateView;
            CustomPlayerLoopInjection.OnPostLateUpdateEnd -= CompliteUpdateView;
        }
    }
}
