using Game.Management;
using System;
using UnityEngine;
using Zenject;

namespace Game.Physics
{
    public class FieldOfViewEntity : MonoBehaviour
    {
        public event Action OnKnowWherePlayerIs;

        [Inject] private FieldOfViewEntitiesController _controller;
        [Inject] private FieldOfViewSystem _system;

        [SerializeField] private float _fov = 90;
        [SerializeField] private int _rayCount = 2;
        [SerializeField] private float _viewDistance = 500f;

        private bool _awakeCalled = false;

        private bool _wasStartCalled = false;

        public float ViewDistance => _viewDistance;

        private void Awake()
        {
            _awakeCalled = true;
        }

        private void OnEnable()
        {
            if(_wasStartCalled)
            {
                EnableEntity();
            }
        }

        private void OnDisable()
        {
            if(GameManager.IsGameQuitungOrSceneUnloading(gameObject))
                return;

            DisableEntity();
        }

        private void Start()
        {
            if (!_wasStartCalled)
            {
                EnableEntity();
                _wasStartCalled = true;
            }

            _controller.SubscribeTriggerEvents(this);
        }

        private void OnDestroy()
        {
            _controller.UnsubscribeTriggerEvents(this);
        }

        public FovEntityData GetData(int rayBeforeCount, int vertciesBeforeCount, float meshMoveZ)
        {
            return new()
            {
                rayOrigin = transform.position,
                rayDistance = _viewDistance,
                rayCount = _rayCount,
                fovAnlge = _fov,
                worldAngleAdd = transform.eulerAngles.z,
                rayBeforeCount = rayBeforeCount,
                vertciesBeforeCount = vertciesBeforeCount,
                meshMoveZ = meshMoveZ
            };
        }

        public void TriggerEnter2D(Collider2D collision)
        {
            _system.DoWhenJobCompleted(() => _system.AddCollider(this, collision));
        }

        public void TriggerExit2D(Collider2D collision)
        {
            _system.DoWhenJobCompleted(() => _system.RemoveCollider(this, collision));
        }

        public void EnableEntity()
        {
            _system.DoWhenJobCompleted(() => _system.AddEntity(this));
        }

        public void DisableEntity()
        {
            _system.DoWhenJobCompleted(() => _system.RemoveEntity(this));
        }

        public void OnPlayerFound()
        {
            //OnKnowWherePlayerIs?.Invoke();
        }

        public void OnEnemyNotInGuardStateFound()
        {
            //OnKnowWherePlayerIs?.Invoke();
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
