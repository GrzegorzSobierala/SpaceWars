using Game.Combat;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Game.Room.Enemy
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserBeam : MonoBehaviour
    {
        [Inject] EnemyBase _EnemyBase;

        [SerializeField] private float _chargingTime = 0.3f;
        [SerializeField] private float _shootTime = 5f;
        [SerializeField] private float _reloadTime = 5f;
        [SerializeField] private float _range = 300f;
        [SerializeField] private float _chargingWidth = 0.3f;
        [SerializeField] private float _fireWidth = 2f;
        [SerializeField] private float _damage = 1;
        [SerializeField] private float _dealDamageInterval = 0.3f;
        [Space]
        public UnityEvent OnStartReload;
        public UnityEvent OnEndReload;
        [Space]
        [SerializeField] private LayerMask _blockAimLayerMask;
        [SerializeField] private GameObject _shootParticles;
        [SerializeField] private GameObject _hitParticles;


        private LineRenderer _lineRenderer;
        private bool _isFiring = false;
        private ContactFilter2D _contactFilter;
        private float _startChargingTime = -100;
        private float _startShootingTime = -100;
        private float _startReloadingTime = -100;
        private float _lastDamageDealtTime = -100;
        private bool _isReloading = false;

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            TryFire();
        }

        public void StartFire()
        {
            if (_isFiring || _startReloadingTime + _reloadTime > Time.time)
                return;

            _isFiring = true;
            _lineRenderer.enabled = true;

            _startChargingTime = Time.time;
            _startShootingTime = Time.time + _chargingTime;
        }

        public void StopFire()
        {
            if (!_isFiring)
                return;

            _isFiring = false;
            _lineRenderer.enabled = false;
            _startReloadingTime = Time.time;
        }

        private void TryFire()
        {
            if (_startReloadingTime + _reloadTime > Time.time)
            {
                OnReloading();
            }
            if (_startChargingTime + _chargingTime > Time.time)
            {
                OnCharging();
            }
            else if (_startShootingTime + _shootTime > Time.time)
            {
                OnFiring();
            }
            else
            {
                OnIdle();
            }

            
            SetReloadMarker(_startReloadingTime + _reloadTime > Time.time);
        }

        private void OnReloading()
        {
            StopFire();

            SetReloadMarker(true);
            _hitParticles.SetActive(false);
            _shootParticles.SetActive(false);
        }

        private void OnCharging()
        {
            _lineRenderer.startWidth = _chargingWidth;
            _lineRenderer.endWidth = _chargingWidth;

            SetBeamPosition();
            _hitParticles.SetActive(false);
            _shootParticles.SetActive(false);
        }

        private void OnFiring()
        {
            _lineRenderer.startWidth = _fireWidth;
            _lineRenderer.endWidth = _fireWidth;

            RaycastHit2D raycastHit = SetBeamPosition();

            if (_lastDamageDealtTime + _dealDamageInterval > Time.time)
                return;

            if (raycastHit.collider == null)
            {
                _hitParticles.SetActive(false);
                return;
            }
            else
            {
                _hitParticles.transform.position = raycastHit.point;
                _hitParticles.transform.LookAt(transform.position);
                _hitParticles.SetActive(true);
            }

            IHittable[] hittables = raycastHit.collider.GetComponents<IHittable>();
            foreach (IHittable hittable in hittables)
            {
                if (hittable == null)
                    continue;

                Vector2 hitPoint = raycastHit.point;
                DamageData damage = new DamageData(_EnemyBase.gameObject, _damage, hitPoint);

                _lastDamageDealtTime = Time.time;
                hittable.GetHit(damage);

                
            }
            _shootParticles.SetActive(true);
        }

        private void OnIdle()
        {
            StopFire();
            _hitParticles.SetActive(false);
            _shootParticles.SetActive(false);
        }

        private RaycastHit2D SetBeamPosition()
        {
            Vector3 pos = transform.position;
            Vector2 dir = transform.forward;

            RaycastHit2D[] raycastHits = new RaycastHit2D[1];
            Physics2D.Raycast(pos, dir, _contactFilter, raycastHits, _range);

            Vector2 targetPos;
            if (raycastHits[0].collider != null)
            {
                targetPos = raycastHits[0].point;
            }
            else
            {
                targetPos = transform.position + transform.forward * _range;
            }

            Vector3[] positions = new Vector3[2];
            positions[0] = transform.position;
            positions[1] = targetPos;

            _lineRenderer.SetPositions(positions);

            return raycastHits[0];
        }

        private void Initialize()
        {
            _lineRenderer = GetComponent<LineRenderer>();

            _contactFilter = new ContactFilter2D
            {
                useTriggers = false,
                layerMask = _blockAimLayerMask,
                useLayerMask = true,
            };
        }

        private void SetReloadMarker(bool active)
        {
            if (_isReloading == active)
                return;

            _isReloading = active;

            if(active)
            {
                OnStartReload?.Invoke();
            }
            else
            {
                OnEndReload?.Invoke();
            }
        }
    }
}
