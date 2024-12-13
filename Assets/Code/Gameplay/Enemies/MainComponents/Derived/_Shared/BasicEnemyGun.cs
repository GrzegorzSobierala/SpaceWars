using Game.Combat;
using Game.Management;
using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Zenject;

namespace Game.Room.Enemy
{
    public class BasicEnemyGun : EnemyGunBase
    {
        public event Action OnStartReload;
        public event Action OnStopReload;

        [Inject] private PlayerManager _playerManager;

        [SerializeField, AutoFill] private Transform _gunShootPoint;
        [SerializeField] private ShootableObjectBase _bulletPrototype;
        [Space, Header("Aim")]
        [SerializeField] private LayerMask _blockAimLayerMask;
        [SerializeField] private float _gunTraverse = 45f;
        [SerializeField] private float _rotateSpeed = 100f;
        [SerializeField] private float _aimedAngle = 4;
        [SerializeField] private float _aimRange = 500f;
        [SerializeField] private float _aimFollowTime = 2f;
        [SerializeField] LostTargetMode _lostTargetMode;
        [Space, Header("Shoot")]
        [SerializeField] private float _shotInterval = 0.5f;
        [SerializeField] private int _magCapacity = 5;
        [SerializeField] private float _reloadTime = 7f;
        [SerializeField] private float _shootAtMaxDistanceMutli = 0.7f;
        [SerializeField] private float _beforeReloadEventTime = 0.5f;
        [Space]
        [SerializeField] private UnityEvent _onBeforeReloaded;
        [SerializeField] private UnityEvent _onShootGun;

        private Coroutine _reloadCoroutine;
        private float _lastShotTime = 0f;
        private float _endReloadTime = 0f;
        private int _currenaMagAmmo = 0;
        private bool _wasOnBeforeReloadedCalled = false;
        private ContactFilter2D _contactFilter;
        private Vector3 _startForwardDir;

        protected override void Awake()
        {
            base.Awake();

            _currenaMagAmmo = _magCapacity;
            Initalize();
        }

        public override void Prepare()
        {
            _lastShotTime = Time.time;
        }

        protected override void OnAimingAt(Transform target)
        {
            base.OnAimingAt(target);

            if (IsKnowWherePlayerIs(target.position, _aimRange, _aimFollowTime, _contactFilter))
            {
                Aim(target.position, _gunTraverse, _rotateSpeed, _aimedAngle, true);
            }
            else
            {
                Vector2 lookForwardPoint = _rotationTrans.transform.up + _rotationTrans.transform.position;
                Aim(lookForwardPoint, _gunTraverse, _rotateSpeed, _aimedAngle, false);
            }
        }

        protected override void OnStopAiming()
        {
            base.OnStopAiming();
        }

        protected override void OnShooting()
        {
            base.OnShooting();

            TryShoot();
        }

        protected override void OnStopShooting()
        {
            base.OnStopShooting();

            StartReloading();
        }

        private void Initalize()
        {
            _contactFilter = new ContactFilter2D
            {
                useTriggers = false,
                layerMask = _blockAimLayerMask,
                useLayerMask = true,
            };

            _startForwardDir = transform.forward;
        }

        [Button]
        private void Shoot()
        {
            _lastShotTime = Time.time;

            GameObject damageDealer = _body.gameObject;
            Transform parent = _playerManager.transform;

            _onShootGun?.Invoke();

            _bulletPrototype.CreateCopy(damageDealer, parent).Shoot(_body, _gunShootPoint);

            _currenaMagAmmo--;

            OnShoot?.Invoke();

            if (_currenaMagAmmo == 0)
            {
                StartReloading();
            }
        }

        private void TryShoot()
        {
            if (Time.time - _lastShotTime < _shotInterval || _currenaMagAmmo <= 0)
                return;

            if (!IsAimedAtPlayer)
                return;

            float targetDistance = Vector2.Distance(_playerManager.PlayerBody.position, 
                transform.position);
            if (targetDistance > _bulletPrototype.MaxDistance * _shootAtMaxDistanceMutli)
                return;

            Shoot();
        }

        private bool TryReload()
        {
            if (!_wasOnBeforeReloadedCalled && Time.time > _endReloadTime - _beforeReloadEventTime)
            {
                _onBeforeReloaded?.Invoke();
                _wasOnBeforeReloadedCalled = true;
            }

            if (Time.time < _endReloadTime)
                return false;

            Reload();
            return true;
        }

        private void Reload()
        {
            _currenaMagAmmo = _magCapacity;
            OnStopReload?.Invoke();
            _wasOnBeforeReloadedCalled = true;
        }

        private void StartReloading()
        {
            if (_reloadCoroutine != null)
            {
                Debug.Log($"Reloading is already in progress. Time left {Time.time - _endReloadTime}");
                return;
            }

            _endReloadTime = Time.time + _reloadTime;
            OnStartReload?.Invoke();
            StartCoroutine(ReloadingMag());
        }

        private IEnumerator ReloadingMag()
        {
            yield return new WaitUntil(TryReload);

            _reloadCoroutine = null;
        }

        protected void LostTargetAction(LostTargetMode lostTargetMode)
        {
            switch (lostTargetMode)
            {
                case LostTargetMode.Stay:
                    LostTargetStay();
                    break;
                case LostTargetMode.Forward:
                    LostTargetActionForward();
                    break;
                case LostTargetMode.Search:
                    LostTargetActionSearch();
                    break;
                default:
                    Debug.LogError("Switch error");
                    break;
            }
        }

        private void LostTargetStay()
        {

        }

        private void LostTargetActionForward()
        {
            Aim(_startForwardDir, 360, _rotateSpeed, _aimedAngle, false);
        }

        private void LostTargetActionSearch()
        {

        }
    }
}

