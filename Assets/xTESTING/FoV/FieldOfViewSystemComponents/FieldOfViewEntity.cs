using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Physics
{
    public class FieldOfViewEntity : MonoBehaviour
    {
        [SerializeField] private float _fov = 90;
        [SerializeField] private int _rayCount = 2;
        [SerializeField] private float _viewDistance = 500f;

        private FieldOfViewSystem _system;
        private bool _awakeCalled = false;
        //private Collider2D[] _overlapColliders;

        //private List<Collider2D> _overlapCashe = new();

        private void Awake()
        {
            _system = GetComponentInParent<FieldOfViewSystem>();
            //_overlapColliders = GetComponents<Collider2D>();
            _awakeCalled = true;
        }

        private void OnEnable()
        {
            _system.AddEntity(this);
        }

        private void OnDisable()
        {
            _system.RemoveEntity(this);
        }

        private void Start()
        {
            //foreach (var collider in _overlapColliders)
            //{
            //    collider.OverlapCollider(_system.ContactFilter, _overlapCashe);
            //    foreach (var overlaped in _overlapCashe)
            //    {
            //        _system.AddCollider(this, overlaped);
            //    }
            //}
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            _system.AddCollider(this, collision);
            //Debug.Log("enter");
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            _system.RemoveCollider(this, collision);
            //Debug.Log("exit");
        }

        public FovEntityData GetData(int rayBeforeCount, int vertciesBeforeCount)
        {
            return new()
            {
                rayOrigin = transform.position,
                rayDistance = _viewDistance,
                rayCount = _rayCount,
                fovAnlge = _fov,
                worldAngleAdd = transform.eulerAngles.z,
                rayBeforeCount = rayBeforeCount,
                vertciesBeforeCount = vertciesBeforeCount
            };
        }

        #region EDITOR

        private void OnValidate()
        {
            if(!Application.isPlaying || !_awakeCalled)
                return;

            //Need for ray count change
            _system.OnEntityDataChange();
        }

        #endregion
    }
}
