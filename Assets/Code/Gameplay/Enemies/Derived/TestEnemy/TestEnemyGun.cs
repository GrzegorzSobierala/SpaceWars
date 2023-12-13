using Game.Management;
using Game.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class TestEnemyGun : EnemyGunBase
    {
        private Vector2 PlayerPos => _playerManager.PlayerBody.position;

        [Inject] private PlayerManager _playerManager;
        [Inject] private EnemyManager _enemyManager;

        [SerializeField] private Transform _gunTransform;
        [SerializeField] private EnemyBullet _enemyBulletPrototype;
        [SerializeField] private float _cooldown = 2f;

        private float _lastShotTime = 0f;
        private bool _isShooting = false;
        private bool _isTransformAiming = false;
        private bool _isPosAiming = false;
        private bool _isRotAiming = false;

        private Transform _aimTargetTransform;
        private Vector2 _aimTargetPos;
        private float _aimTargetRot;

        private void Update()
        {
            AimGun();

            if (_isShooting)
            {
                TryShoot();
            }
        }

        public override void StartAimingAt(Transform target)
        {
            _isTransformAiming = true;

            _isPosAiming = false;
            _isRotAiming = false;

            _aimTargetTransform = target;
        }

        public override void StartAimingAt(Vector2 worldPosition)
        {
            _isPosAiming = true;

            _isTransformAiming = false;
            _isRotAiming = false;

            _aimTargetPos = worldPosition;
        }

        public override void StartAimingAt(float localRotation)
        {
            _isRotAiming = true;

            _isPosAiming = false;
            _isTransformAiming = false;

            _aimTargetRot = localRotation;
        }

        public override void StopAiming()
        {
            _isRotAiming = false;
            _isPosAiming = false;
            _isTransformAiming = false;
        }

        public override void StartShooting()
        {
            _isShooting = true;
        }

        public override void StopShooting()
        {
            _isShooting = false;
        }

        private void TryShoot()
        {
            if (Vector2.Distance(transform.position, PlayerPos) > _enemyBulletPrototype.MaxDistance)
                return;

            if (Time.time - _lastShotTime < _cooldown)
            {
                return;
            }

            Shoot();
        }

        private void Shoot()
        {
            _lastShotTime = Time.time;

            _enemyBulletPrototype.CreateCopy(_enemyManager.transform).Shoot(null, _gunTransform);
        }

        private void AimGun()
        {
            Vector2 gunPos = (Vector2)_gunTransform.position;
            float angleDegrees;

            if (_isTransformAiming)
            {
                angleDegrees = Utils.AngleDirected(gunPos, _aimTargetTransform.position) - 90f;
            }
            else if(_isPosAiming)
            {
                angleDegrees = Utils.AngleDirected(gunPos, _aimTargetPos) - 90f;
            }
            else if (_isRotAiming)
            {
                angleDegrees = _aimTargetRot;
            }
            else
            {
                return;
            }

            Quaternion rotation = Quaternion.Euler(0, 0, angleDegrees);
            _gunTransform.rotation = rotation;

            OnAimTarget?.Invoke();
        }
    }
}
