using CodeMonkey.Utils;
using Game.Management;
using Game.Utility;
using Game.Utility.Globals;
using System;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    [RequireComponent(typeof(MeshFilter))]
    public class EnemyFieldOfView : MonoBehaviour
    {
        public Action<GameObject> OnTargetFound;

        [Inject] private PlayerManager _playerManager;
        [Inject] private Rigidbody2D _body;

        [SerializeField] private float _fov = 90;
        [SerializeField] private int _rayCount = 2;
        [SerializeField] private float _viewDistance = 500f;

        private float AngleIncrease => _fov / _rayCount;
        private const float PlayerCameraMaxViewDistance = 500f;

        private MeshFilter _meshFilter;
        private Mesh _mesh;

        private LayerMask _allLayerMask;
        private LayerMask _targetLayerMask;


        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (!IsPlayerInRange())
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
            _allLayerMask = LayerMask.GetMask(Layers.Player, Layers.Obstacle);
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

            ContactFilter2D contactFilter = new ContactFilter2D
            {
                useTriggers = false,
                layerMask = _allLayerMask,
                useLayerMask = true
            };

            for (int i = 0; i <= _rayCount; i++)
            {
                Vector3 vertex;
                Vector3 rayDirection = UtilsClass.GetVectorFromAngle(currentAngle + worldAngleAdd);
                Vector3 origin = transform.position;

                RaycastHit2D[] raycastHits = new RaycastHit2D[1];

                int count = Physics2D.Raycast(origin, rayDirection, contactFilter,
                    raycastHits, _viewDistance);

                if (count == 0)
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
                    vertex = raycastHits[0].point - (Vector2)transform.position;
                    vertex = Utils.RotateVector(vertex, -worldAngleAdd);

                    if ((_targetLayerMask & (1 << raycastHits[0].collider.gameObject.layer)) != 0)
                    {
                        OnTargetFound?.Invoke(raycastHits[0].collider.gameObject);
                    }
                }

                verticies[vertexIndex] = vertex;

                

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
            }
        }

        private bool IsPlayerInRange()
        {
            Vector2 playerPos = _playerManager.PlayerBody.position;
            Vector2 enemyPos = _body.position;
            float maxDistanceToPlayer = PlayerCameraMaxViewDistance + _viewDistance;

            return Vector2.Distance(playerPos, enemyPos) < maxDistanceToPlayer;
        }
    }
}
