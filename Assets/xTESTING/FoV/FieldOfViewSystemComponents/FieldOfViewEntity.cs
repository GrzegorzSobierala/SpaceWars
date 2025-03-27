using Game.Management;
using UnityEngine;
using Zenject;

namespace Game.Physics
{
    public class FieldOfViewEntity : MonoBehaviour
    {
        [InjectOptional] private FieldOfViewEntitiesController _controller;
        [Inject] private FieldOfViewSystem _system;

        [SerializeField] private float _fov = 90;
        [SerializeField] private int _rayCount = 2;
        [SerializeField] private float _viewDistance = 500f;

        private bool _awakeCalled = false;
        //private Collider2D[] _overlapColliders;

        //private List<Collider2D> _overlapCashe = new();

        private bool _wasStartCalled = false;

        private void Awake()
        {
            //_system = GetComponentInParent<FieldOfViewSystem>();
            //_overlapColliders = GetComponents<Collider2D>();
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

            //foreach (var collider in _overlapColliders)
            //{
            //    collider.OverlapCollider(_system.ContactFilter, _overlapCashe);
            //    foreach (var overlaped in _overlapCashe)
            //    {
            //        _system.AddCollider(this, overlaped);
            //    }
            //}
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
            _system.OnUpdateViewCompleted += () => _system.AddCollider(this, collision);
        }

        public void TriggerExit2D(Collider2D collision)
        {
            _system.OnUpdateViewCompleted += () => _system.RemoveCollider(this, collision);
        }

        public void EnableEntity()
        {
            _system.OnUpdateViewCompleted += () => _system.AddEntity(this);
        }

        public void DisableEntity()
        {
            _system.OnUpdateViewCompleted += () => _system.RemoveEntity(this);
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
