using AYellowpaper.SerializedCollections;
using Game.Utility;
using Game.Utility.Globals;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Jobs.LowLevel.Unsafe;

namespace Game.Physics
{
    public class FieldOfView : MonoBehaviour
    {
        public Action<GameObject> OnTargetFound;

        //[Inject] private PlayerManager _playerManager;
        //[Inject] private Rigidbody2D _body;
        //[Inject] private List<EnemyDamageHandler> _damageHandles;
        //[Inject] private List<EnemyBase> _roomEnemies;

        [SerializeField] public float _fov = 90;
        [SerializeField] public int _rayCount = 2;
        [SerializeField] public float _viewDistance = 500f;
        [SerializeField] public SerializedDictionary<Collider2D, OneEnum> _ignoreColliders;

        private const float PlayerCameraMaxViewDistance = 500f;

        private MeshFilter _meshFilter;
        private Mesh _mesh;
        private Collider2D _overlapCollider;

        private LayerMask _allLayerMask;
        private LayerMask _targetLayerMask;
        private LayerMask _enemyLayerMask;
        private ContactFilter2D _contactFilter;

        private float _randomVertexZ;

        private Vector2[] _pathPointsCompositeCache = new Vector2[10];
        private List<Collider2D> _collidersCashe = new();

        private void Awake()
        {
            Initialize();
        }

        private void /*Fixed*/Update()
        {
            //if (!IsPlayerInRange() && !IsNonGuardEnemyInRange())
            //    return;

            UpdateView();
        }

        public void DrawViewGizmos()
        {
            //UpdateView(true);
        }

        private void Initialize()
        {
            _mesh = new Mesh();
            _meshFilter = GetComponent<MeshFilter>();
            _meshFilter.mesh = _mesh;
            _targetLayerMask = LayerMask.GetMask(Layers.Player);
            _allLayerMask = GetAllLayerMask();
            _enemyLayerMask = LayerMask.GetMask(Layers.Enemy);
            _randomVertexZ = UnityEngine.Random.Range(1.0f, 3.0f);
            _overlapCollider = GetComponent<Collider2D>();

            _contactFilter = new ContactFilter2D
            {
                useTriggers = false,
                layerMask = _allLayerMask,
                useLayerMask = true,
            };
        }

        private void OnValidate()
        {
            if(!Application.isPlaying)
                return;

            if (_mesh)
            {
                _mesh.uv = new Vector2[_rayCount + 1 + 1];
            }
        }

        private LayerMask GetAllLayerMask()
        {
            return LayerMask.GetMask(Layers.Player, Layers.Obstacle, Layers.Enemy);
        }

        private void UpdateView(/*bool debugMode = false*/)
        {
            //Profiler.BeginSample("amigus1-1 over");
            //_overlapCollider.OverlapCollider(_contactFilter, _collidersCashe);
            //Profiler.EndSample();

            //Profiler.BeginSample("amigus1-2 list1");
            //NativeList<ColliderDataUnprepared> datasUnprep = new(_collidersCashe.Count, Allocator.TempJob);
            //Profiler.EndSample();

            //Profiler.BeginSample("amigus1-3 list2");
            //NativeList<Vector2> vertsUnprep = new(_collidersCashe.Count * 5, Allocator.TempJob);
            //Profiler.EndSample();

            //Profiler.BeginSample("amigus1-4 dataUnpare");

            //for (int i = 0; i < _collidersCashe.Count; i++)
            //{
            //    Collider2D col = _collidersCashe[i];
            //    Transform colTrans = col.transform;
            //    switch (col)
            //    {
            //        case BoxCollider2D box:
            //            Profiler.BeginSample("amigus1-4-1 dataUnpare box");

            //            ColliderDataUnprepared boxData = new()
            //            {
            //                typeEnum = ColliderType.Box,
            //                posWorld = colTrans.position,
            //                rotWorld = colTrans.eulerAngles.z,
            //                offsetLoc = box.offset,
            //                lossyScale = colTrans.lossyScale,
            //                sizeLoc = box.size
            //            };
            //            datasUnprep.Add(boxData);
            //            Profiler.EndSample();
            //            break;

            //        case CircleCollider2D circle:
            //            Profiler.BeginSample("amigus1-4-2 dataUnpare circle");

            //            ColliderDataUnprepared circleData = new()
            //            {
            //                typeEnum = ColliderType.Circle,
            //                posWorld = colTrans.position,
            //                rotWorld = colTrans.eulerAngles.z,
            //                offsetLoc = circle.offset,
            //                lossyScale = colTrans.localScale,
            //                radiusLoc = circle.radius
            //            };
            //            datasUnprep.Add(circleData);
            //            Profiler.EndSample();
            //            break;

            //        case CapsuleCollider2D capsule:
            //            Profiler.BeginSample("amigus1-4-3 dataUnpare capsule");
            //            ColliderDataUnprepared capsuleData = new()
            //            {
            //                typeEnum = ColliderType.Capsule,
            //                offsetLoc = capsule.offset,
            //                posWorld = colTrans.position,
            //                rotWorld = colTrans.eulerAngles.z,
            //                lossyScale = colTrans.lossyScale,
            //                sizeLoc = capsule.size,
            //                capsuleDirEnum = capsule.direction,
            //                capsuleTransUpOrBoundsPos = colTrans.up,
            //                capsuleTransRightOrBoundsSize = colTrans.right
            //            };
            //            datasUnprep.Add(capsuleData);
            //            Profiler.EndSample();
            //            break;

            //        case PolygonCollider2D poly:
            //            Profiler.BeginSample("amigus1-4-4 dataUnpare poly");
            //            Vector2[] points = poly.points;
            //            //Profiler.EndSample();

            //            //Profiler.BeginSample("amigus polygon 2");
            //            ColliderDataUnprepared polyData = new()
            //            {
            //                typeEnum = ColliderType.Polygon,
            //                vertexStartIndex = vertsUnprep.Length,
            //                posWorld = colTrans.position,
            //                rotWorld = colTrans.eulerAngles.z,
            //                lossyScale = colTrans.lossyScale,
            //                vertexCount = points.Length,
            //            };
            //            //Profiler.EndSample();

            //            //Profiler.BeginSample("amigus polygon 3");
            //            unsafe
            //            {
            //                fixed (Vector2* ptr = points)
            //                {
            //                    vertsUnprep.AddRange(ptr, points.Length);
            //                }
            //            }

            //            //Profiler.EndSample();
            //            //Profiler.BeginSample("amigus polygon 4");
            //            datasUnprep.Add(polyData);
            //            Profiler.EndSample();
            //            break;

            //        case EdgeCollider2D edge:
            //            Profiler.BeginSample("amigus1-4-5 dataUnpare edge");
            //            Vector2[] edgePoints = edge.points;
            //            ColliderDataUnprepared edgeData = new()
            //            {
            //                typeEnum = ColliderType.Edge,
            //                vertexStartIndex = vertsUnprep.Length,
            //                posWorld = colTrans.position,
            //                rotWorld = colTrans.eulerAngles.z,
            //                lossyScale = colTrans.lossyScale,
            //                vertexCount = edgePoints.Length,
            //            };

            //            unsafe
            //            {
            //                fixed (Vector2* ptr = edgePoints)
            //                {
            //                    vertsUnprep.AddRange(ptr, edgePoints.Length);
            //                }
            //            }

            //            datasUnprep.Add(edgeData);
            //            Profiler.EndSample();
            //            break;

            //        case CompositeCollider2D composite:
            //            Profiler.BeginSample("amigus1-4-6 dataUnpare composite");
            //            for (int p = 0; p < composite.pathCount; p++)
            //            {
            //                Profiler.BeginSample("amigus composite 1");
            //                int pointCount = composite.GetPathPointCount(p);
            //                Profiler.EndSample();

            //                Profiler.BeginSample("amigus composite 2");
            //                ColliderDataUnprepared compositeData = new()
            //                {
            //                    typeEnum = ColliderType.Composite,
            //                    vertexStartIndex = vertsUnprep.Length,
            //                    posWorld = colTrans.position,
            //                    rotWorld = colTrans.eulerAngles.z,
            //                    vertexCount = pointCount,
            //                };

            //                Profiler.EndSample();

            //                Profiler.BeginSample("amigus composite 3");
            //                unsafe
            //                {
            //                    if (_pathPointsCompositeCache.Length < pointCount)
            //                    {
            //                        _pathPointsCompositeCache = new Vector2[pointCount];
            //                    }
            //                    composite.GetPath(p, _pathPointsCompositeCache);
            //                    fixed (Vector2* pathPointsPtr = _pathPointsCompositeCache)
            //                    {
            //                        vertsUnprep.AddRange(pathPointsPtr, pointCount);
            //                    }
            //                }

            //                Profiler.EndSample();

            //                Profiler.BeginSample("amigus composite 4");
            //                datasUnprep.Add(compositeData);
            //                Profiler.EndSample();
            //            }
            //            Profiler.EndSample();
            //            break;

            //        default:
            //            ColliderDataUnprepared defaultData = new()
            //            {
            //                typeEnum = ColliderType.Unsuported,
            //                posWorld = col.bounds.center,
            //                sizeLoc = col.bounds.size,
            //            };
            //            datasUnprep.Add(defaultData);
            //            Debug.LogError("Unsuported collider type");
            //            break;

            //    }
            //}
            //Profiler.EndSample();

            //Profiler.BeginSample("amigus1-5 dataPrepare list1");
            //NativeArray<ColliderDataReady> datasRdy = new(datasUnprep.Length, Allocator.TempJob);
            //Profiler.EndSample();

            //Profiler.BeginSample("amigus1-6 dataPrepare list2");
            //NativeArray<float2> vertsRdy = new(vertsUnprep.Length, Allocator.TempJob);
            //Profiler.EndSample();

            //Profiler.BeginSample("amigus1-7-1 dataPrepare job");
            //DEPRECATED_PrepareColliderDatasJob prepareJob = new DEPRECATED_PrepareColliderDatasJob
            //{
            //    datasUnprep = datasUnprep,
            //    vertsUnprep = vertsUnprep,
            //    datasRdy = datasRdy,
            //    vertsRdy = vertsRdy,
            //};
            //Profiler.EndSample();

            //Profiler.BeginSample("amigus1-7-2 dataPrepare job");
            //prepareJob.Run(datasRdy.Length);
            //Profiler.EndSample();

            //Profiler.BeginSample("amigus1-8 dataPrepare dispose");
            //datasUnprep.Dispose();
            //vertsUnprep.Dispose();
            //Profiler.EndSample();

            //Profiler.BeginSample("amigus1-9 rayJob verticies array");
            //NativeArray<Vector3> verticies = new(_rayCount + 1 + 1, Allocator.TempJob);
            //verticies[0] = Vector3.zero;
            //Profiler.EndSample();

            //Profiler.BeginSample("amigus2-2 rayJob triangles array");
            //NativeArray<int> triangles = new(_rayCount * 3, Allocator.TempJob);
            //Profiler.EndSample();

            //Profiler.BeginSample("amigus2-3 rayJob create");
            //DEPRECATED_Raycast2DWithMeshJob raycastsJob = new DEPRECATED_Raycast2DWithMeshJob
            //{
            //    rayOrigin = transform.position,
            //    rayDistance = _viewDistance,
            //    rayCount = _rayCount,
            //    fovAnlge = _fov,
            //    worldAngleAdd = transform.eulerAngles.z,
            //    colliderDataArray = datasRdy,
            //    vertexArray = vertsRdy,
            //    verticies = verticies,
            //    triangles = triangles,
            //};
            //Profiler.EndSample();

            //Profiler.BeginSample("amigus2-4 rayJob run");
            //int batchCount = Mathf.Max(1, _rayCount / (JobsUtility.JobWorkerCount * 2));
            //JobHandle raycastsJobHandle = raycastsJob.Schedule(_rayCount, batchCount);
            //raycastsJobHandle.Complete();
            //Profiler.EndSample();

            //Profiler.BeginSample("amigus2-5 mesh vertices");
            //_mesh.SetVertices(verticies);
            //Profiler.EndSample();
            //Profiler.BeginSample("amigus2-7 mesh triangles");
            //_mesh.triangles = triangles.ToArray();
            ////_mesh.SetTriangles()
            //Profiler.EndSample();

            //Profiler.BeginSample("amigus2-8 rayJob dispose");
            //datasRdy.Dispose();
            //vertsRdy.Dispose();
            //verticies.Dispose();
            //triangles.Dispose();
            //Profiler.EndSample();
        }
    }
}
