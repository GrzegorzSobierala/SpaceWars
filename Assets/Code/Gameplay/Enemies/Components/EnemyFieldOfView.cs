using CodeMonkey.Utils;
using Game.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [ExecuteInEditMode]
    public class EnemyFieldOfView : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private Vector3 _offset = Vector3.zero;
        [SerializeField] private float _fov = 90;
        [SerializeField] private int _rayCount = 2;
        [SerializeField] private float _viewDistance = 500f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _startAngle;
        private float AngleIncrease => _fov / _rayCount;

        private Mesh _mesh;

        private void Start()
        {
            _mesh = new Mesh();
            _meshFilter.mesh = _mesh;
        }

        private void Update()
        {
            float currentAngle = -_fov / 2;
            float worldAngleAdd = transform.rotation.eulerAngles.z;

            Vector3[] verticies = new Vector3[_rayCount + 1 + 1];
            Vector2[] uv = new Vector2[verticies.Length];
            int[] triangles = new int[_rayCount * 3];
            verticies[0] = _offset;

            int vertexIndex = 1;
            int triangleIndex = 0;
            for (int i = 0; i <= _rayCount; i++)
            {
                Vector3 vertex;
                Vector3 rayDirection = UtilsClass.GetVectorFromAngle(currentAngle + worldAngleAdd);
                Vector3 origin = transform.position + _offset;

                RaycastHit2D[] raycastHits = new RaycastHit2D[1];

                ContactFilter2D contactFilter = new ContactFilter2D
                {
                    useTriggers = false,
                    layerMask = _layerMask,
                    useLayerMask = true
                };

                int count = Physics2D.Raycast(origin, rayDirection, contactFilter, raycastHits, _viewDistance);

                if (count == 0)
                {
                    Vector3 direction = UtilsClass.GetVectorFromAngle(currentAngle);
                    vertex = _offset + direction * _viewDistance;
                }
                else
                {
                    vertex = raycastHits[0].point - (Vector2)transform.position ;
                    vertex = Utils.RotateVector(vertex, -worldAngleAdd);
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

            _mesh.vertices = verticies;
            _mesh.uv = uv;
            _mesh.triangles = triangles;
        }

        private void Set()
        {
            _startAngle = UtilsClass.GetAngleFromVectorFloat(transform.forward) - _fov / 2;
        }
    }
}
