using Game.Combat;
using Game.Management;
using Game.Room.Enemy;
using Game.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game
{
    public class CursorEnemyGun : EnemyGunBase
    {
        [Inject] private PlayerManager _playerManager;
        [Inject] private EnemyManager _enemyManager;
        [Inject] private Rigidbody2D _body;

        [SerializeField] private Transform _gunTransform;
        [SerializeField] private ShootableObjectBase _bulletPrototype;
        [SerializeField] private float _shotInterval = 0.5f;
        [SerializeField] private int _magCapasity = 5;
        [SerializeField] private float _reloadTime = 7f;
        [SerializeField] private float _gunTravers = 45f;
        [SerializeField] private float _shootAtMaxDistanceMutli = 0.7f;

        private Action OnStartReload;
        private Action OnStopReload;

        private Coroutine _reloadCoroutine;
        private float _lastShotTime = 0f;
        private float _endReloadTime = 0f;
        private int _currenaMagAmmo = 0;
        private bool _isAimed = false;


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
            OnStartReload += onReloadAction;
        }

        public void UnsubscribeOnStartReload(Action onReloadAction)
        {
            OnStartReload -= onReloadAction;
        }

        public void SubscribeOnStopReload(Action onReloadAction)
        {
            OnStopReload += onReloadAction;
        }

        public void UnsubscribeOnStopReload(Action onReloadAction)
        {
            OnStopReload -= onReloadAction;
        }

        protected override void OnAimingAt(Transform target)
        {
            Vector2 gunPos = (Vector2)_gunTransform.position;

            Vector2 vectorToTarget = target.position - transform.position;
            float angleToTarget = Vector2.SignedAngle(_body.transform.up, vectorToTarget);

            float newAngle;

            if (angleToTarget >= -_gunTravers / 2 && angleToTarget <= _gunTravers / 2)
            {
                OnAimTarget?.Invoke();
                _isAimed = true;
            }
            else
            {
                _isAimed = false;
            }
            Debug.Log(_isAimed);

            newAngle = Mathf.Clamp(angleToTarget, -_gunTravers / 2, _gunTravers / 2);

            _gunTransform.localRotation = Quaternion.Euler(0, 0, newAngle);
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
            _bulletPrototype.CreateCopy(damageDealer, parent).Shoot(_body, transform);

            _currenaMagAmmo--;

            if(_currenaMagAmmo == 0)
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
            if (Time.time < _endReloadTime)
                return false;

            Reload();
            return true;
        }

        private void Reload()
        {
            _currenaMagAmmo = _magCapasity;
            OnStopReload?.Invoke();
        }

        private void StartReloading()
        {
            if(_reloadCoroutine != null)
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
            while(!TryReload()) 
            {
                yield return null;
            }
            _reloadCoroutine = null;
        }
    }
}
