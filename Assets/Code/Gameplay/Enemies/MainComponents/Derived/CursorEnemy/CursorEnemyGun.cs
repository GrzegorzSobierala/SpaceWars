using Game.Combat;
using Game.Management;
using Game.Room.Enemy;
using Game.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Game
{
    public class CursorEnemyGun : EnemyGunBase
    {
        [Inject] private PlayerManager _playerManager;
        [Inject] private EnemyManager _enemyManager;
        [Inject] private Rigidbody2D _body;

        [SerializeField] private Transform _leftGunTransform;
        [SerializeField] private Transform _leftGunShootPoint;
        [SerializeField] private Transform _rightGunTransform;
        [SerializeField] private Transform _rightGunShootPoint;
        [SerializeField] private ShootableObjectBase _bulletPrototype;
        [SerializeField] private float _shotInterval = 0.5f;
        [SerializeField] private int _magCapasity = 5;
        [SerializeField] private float _reloadTime = 7f;
        [SerializeField] private float _gunTravers = 45f;
        [SerializeField] private float _shootAtMaxDistanceMutli = 0.7f;
        [SerializeField] private float _beforeReloadEventTime = 0.5f;

        [SerializeField] private UnityEvent _onBeforeReloaded;
        [SerializeField] private UnityEvent _onShootLeftGun; 
        [SerializeField] private UnityEvent _onShootRightGun;

        private Action _onStartReload;
        private Action _onStopReload;

        private Coroutine _reloadCoroutine;
        private float _lastShotTime = 0f;
        private float _endReloadTime = 0f;
        private int _currenaMagAmmo = 0;
        private bool _isAimed = false;
        private bool _isAimingLeft = false;
        private bool _wasOnBeforeReloadedCalled = false;

        private void Awake()
        {
            _currenaMagAmmo = _magCapasity;
        }

        public override void Prepare()
        {
            _lastShotTime = Time.time;
        }

        public void SubscribeOnStartReload(Action onReloadAction)
        {
            _onStartReload += onReloadAction;
        }

        public void UnsubscribeOnStartReload(Action onReloadAction)
        {
            _onStartReload -= onReloadAction;
        }

        public void SubscribeOnStopReload(Action onReloadAction)
        {
            _onStopReload += onReloadAction;
        }

        public void UnsubscribeOnStopReload(Action onReloadAction)
        {
            _onStopReload -= onReloadAction;
        }

        protected override void OnAimingAt(Transform target)
        {
            Vector2 vectorToTarget = target.position - transform.position;
            float angleToTarget = Vector2.SignedAngle(_body.transform.up, vectorToTarget);

            if (angleToTarget >= -_gunTravers / 2 && angleToTarget <= _gunTravers / 2)
            {
                OnAimTarget?.Invoke();
                _isAimed = true;
            }
            else
            {
                _isAimed = false;
            }

            float newLeftAngle = Mathf.Clamp(angleToTarget, 0, _gunTravers / 2);
            float newRightAngle = Mathf.Clamp(angleToTarget, -_gunTravers / 2, 0);

            _leftGunTransform.localRotation = Quaternion.Euler(0, 0, newLeftAngle);
            _rightGunTransform.localRotation = Quaternion.Euler(0, 0, newRightAngle);

            _isAimingLeft = newLeftAngle != 0;
        }

        protected override void OnStopAiming()
        {
            base.OnStopAiming();

            _isAimed = false;
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

        private void Shoot()
        {
            _lastShotTime = Time.time;

            GameObject damageDealer = _body.gameObject;
            Transform parent = _playerManager.transform;

            Transform shootTransform = _isAimingLeft ? _leftGunShootPoint : _rightGunShootPoint;

            if (_isAimingLeft)
            {
                _onShootLeftGun?.Invoke();
            }
            else
            {
                _onShootRightGun?.Invoke();
            }

            _bulletPrototype.CreateCopy(damageDealer, parent).Shoot(_body, shootTransform);

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

            if (!_isAimed)
                return;

            float targetDistance = Vector2.Distance(_playerManager.PlayerBody.position, transform.position);
            if (targetDistance > _bulletPrototype.MaxDistance * _shootAtMaxDistanceMutli)
                return;

            Shoot();
        }

        private bool TryReload()
        {
            if(!_wasOnBeforeReloadedCalled && Time.time > _endReloadTime - _beforeReloadEventTime)
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
            _currenaMagAmmo = _magCapasity;
            _onStopReload?.Invoke();
            _wasOnBeforeReloadedCalled = true;
        }

        private void StartReloading()
        {
            if(_reloadCoroutine != null)
            {
                Debug.Log($"Reloading is already in progress. Time left {Time.time - _endReloadTime}");
                return;
            }

            _endReloadTime = Time.time + _reloadTime;
            _onStartReload?.Invoke();
            StartCoroutine(ReloadingMag());
        }

        private IEnumerator ReloadingMag()
        {
            while(!TryReload()) 
            {
                yield return null;
            }
            _reloadCoroutine = null;
        }
    }
}
