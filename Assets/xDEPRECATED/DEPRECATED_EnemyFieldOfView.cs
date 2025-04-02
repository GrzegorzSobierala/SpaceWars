using AYellowpaper.SerializedCollections;
using CodeMonkey.Utils;
using Game.Management;
using Game.Utility;
using Game.Utility.Globals;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    //[RequireComponent(typeof(MeshFilter))]
    public class DEPRECATED_EnemyFieldOfView : MonoBehaviour
    {
        public Action<GameObject> OnTargetFound;

        [Inject] private PlayerManager _playerManager;
        [Inject] private Rigidbody2D _body;
        [Inject] private List<EnemyDamageHandler> _damageHandles;
        [Inject] private List<EnemyBase> _roomEnemies;

        [SerializeField] private float _fov = 90;
        [SerializeField] private int _rayCount = 2;
        [SerializeField] private float _viewDistance = 500f;
        [SerializeField] private bool _queriesStartInColliders = false;
        [SerializeField] private SerializedDictionary<Collider2D,OneEnum> _ignoreColliders;

        private static HashSet<Collider2D> _DEBUG_wrongLayerColliders = new();

        private float AngleIncrease => _fov / _rayCount;
        private const float PlayerCameraMaxViewDistance = 500f;

        private MeshFilter _meshFilter;
        private Mesh _mesh;

        private LayerMask _allLayerMask;
        private LayerMask _targetLayerMask;
        private LayerMask _enemyLayerMask;

        private float _randomVertexZ;

        private void Awake()
        {
            Initialize();
        }

        private void FixedUpdate()
        {
            if (!IsPlayerInRange() && !IsNonGuardEnemyInRange())
                return;   
            
            UpdateView();
        }

        public void DrawViewGizmos()
        {
            UpdateView(true);
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
        }

        private LayerMask GetAllLayerMask()
        {
            return LayerMask.GetMask(Layers.Player, Layers.Obstacle, Layers.Enemy);
        }

        private void UpdateView(bool debugMode = false)
        {
            float currentAngle = (_fov / 2) + 90;
            float worldAngleAdd = transform.rotation.eulerAngles.z;

            Vector3[] verticies = new Vector3[_rayCount + 1 + 1];
            Vector2[] uv = new Vector2[verticies.Length];
            int[] triangles = new int[_rayCount * 3];
            verticies[0] = Vector3.zero;

            int vertexIndex = 1;
            int triangleIndex = 0;

            if (!debugMode)
            {
                foreach (var handler in _damageHandles)
                {
                    handler.Collider.enabled = false;
                }
            }

            ContactFilter2D contactFilter = new ContactFilter2D
            {
                useTriggers = false,
                layerMask = _allLayerMask,
                useLayerMask = true,
            };

            bool saveQueriesStartInColliders = Physics2D.queriesStartInColliders;
            Physics2D.queriesStartInColliders = _queriesStartInColliders;

            List<RaycastHit2D> raycastHits = new();
            List<RaycastHit2D> raycastEnemyHits = new();
            Vector3 origin = transform.position;

            for (int i = 0; i <= _rayCount; i++)
            {
                Vector3 rayDirection = UtilsClass.GetVectorFromAngle(currentAngle + worldAngleAdd);

                int hitsCount;

                hitsCount = Physics2D.Raycast(origin, rayDirection, contactFilter,
                raycastHits, _viewDistance);

                RaycastHit2D? firstHit = null;

                for (int j = 0; j < hitsCount; j++)
                {
                    if (_ignoreColliders.ContainsKey(raycastHits[j].collider))
                    {
                        continue;
                    }

                    if ((_enemyLayerMask & (1 << raycastHits[j].collider.gameObject.layer)) != 0)
                    {
                        raycastEnemyHits.Add(raycastHits[j]);
                        continue;
                    }

                    firstHit = raycastHits[j];
                    break;
                }

                Vector3 vertex;
                if (firstHit == null)
                {
                    Vector3 direction = UtilsClass.GetVectorFromAngle(currentAngle);
                    vertex = direction * _viewDistance;

                    if (debugMode)
                    {
                        Vector3 debugDir = Utils.RotateVector(direction, worldAngleAdd);
                        Vector3 endLine = debugDir * _viewDistance + transform.position;
                        Debug.DrawLine(transform.position, endLine);
                    }
                }
                else
                {
                    vertex = firstHit.Value.point - (Vector2)transform.position;
                    vertex = Utils.RotateVector(vertex, -worldAngleAdd);

                    if (IsTargetFound(firstHit.Value, raycastEnemyHits))
                    {
                        OnTargetFound?.Invoke(firstHit.Value.collider.gameObject);
                    }
                }

                verticies[vertexIndex] = vertex + new Vector3(0, 0, _randomVertexZ);

                if (i > 0)
                {
                    triangles[triangleIndex] = 0;
                    triangles[triangleIndex + 1] = vertexIndex - 1;
                    triangles[triangleIndex + 2] = vertexIndex;

                    triangleIndex += 3;
                }

                vertexIndex++;
                currentAngle -= AngleIncrease;
            }

            if(!debugMode)
            {
                _mesh.vertices = verticies;
                _mesh.uv = uv;
                _mesh.triangles = triangles;

                foreach (var handler in _damageHandles)
                {
                    handler.Collider.enabled = true;
                }
            }

            Physics2D.queriesStartInColliders = saveQueriesStartInColliders;
        }

        private bool IsTargetFound(RaycastHit2D hit, List<RaycastHit2D> raycastEnemyHits)
        {
            if ((_targetLayerMask & (1 << hit.collider.gameObject.layer)) != 0)
            {
                return true;
            }

            if(raycastEnemyHits.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < raycastEnemyHits.Count; i++)
            {
                if (raycastEnemyHits[i].collider.
                    TryGetComponent(out IGuardStateDetectable EnemyStateDetectable))
                {
                    if(EnemyStateDetectable.IsEnemyInGuardState)
                    {
                        continue;
                    }

                    return true;
                }
                else
                {
                    if (_DEBUG_wrongLayerColliders.Contains(hit.collider))
                        return false;

                    Debug.LogError($"Collider on {Layers.Enemy} layer hasn't IGuardStateDetectable", hit.collider);
                    _DEBUG_wrongLayerColliders.Add(hit.collider);
                    return false;
                }
            }

            return false;
        }

        private bool IsPlayerInRange()
        {
            Vector2 playerPos = _playerManager.PlayerBody.position;
            Vector2 enemyPos = _body.position;
            float maxDistanceToPlayer = PlayerCameraMaxViewDistance + _viewDistance;

            return Vector2.Distance(playerPos, enemyPos) < maxDistanceToPlayer;
        }

        private bool IsNonGuardEnemyInRange()
        {
            foreach (var enemy in _roomEnemies)
            {
                if (enemy == null)
                    continue;

                if (enemy.StateMachine.CurrentState is EnemyGuardStateBase)
                    continue;

                if (Vector2.Distance(enemy.transform.position, transform.position) > _viewDistance + 100)
                    continue;

                return true;
            }

            return false;
        }

        public void ClearAndAssignColliders()
        {
            EnemyBase enemy = Utils.FindEnemyInParents<EnemyBase>(transform);

            if(enemy == null)
            {
                Debug.LogError($"Can't find {nameof(EnemyBase)} in parents");
                return;
            }

            _ignoreColliders.Clear();

            foreach (var collider in enemy.GetComponentsInChildren<Collider2D>())
            {
                if (!Utils.ContainsLayer(GetAllLayerMask(), collider.gameObject.layer))
                    continue;

                _ignoreColliders.Add(collider, OneEnum.OneEnum);
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
