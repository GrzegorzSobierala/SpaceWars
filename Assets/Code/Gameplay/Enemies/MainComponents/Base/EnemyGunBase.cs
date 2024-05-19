using System;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Room.Enemy
{
    public abstract class EnemyGunBase : MonoBehaviour
    {
        [SerializeField] protected UnityEvent OnShot;

        protected Action OnAimTarget;

        private AimType _currentAimType = AimType.Stop;
        private bool _isShooting = false;

        private Transform _aimTargetTransform;
        private Vector2 _aimTargetPos;
        private float _aimTargetRot;

        protected virtual void Update()
        {
            TryAimGun();

            TryShoot();
        }

        public abstract void Prepare();

        public void StartAimingAt(Transform target)
        {
            _currentAimType = AimType.Transform;

            _aimTargetTransform = target;

            OnStartAimingAt(target);
        }

        public void StartAimingAt(Vector2 worldPosition)
        {
            _currentAimType = AimType.Position;

            _aimTargetPos = worldPosition;

            OnStartAimingAt(worldPosition);
        }

        public void StartAimingAt(float localRotation)
        {
            _currentAimType = AimType.Angle;

            _aimTargetRot = localRotation;

            OnStartAimingAt(localRotation);
        }

        public void StopAiming()
        {
            _currentAimType = AimType.Stop;

            OnStopAiming();
        }

        public void StartShooting()
        {
            _isShooting = true;

            OnStartShooting();
        }

        public void StopShooting()
        {
            _isShooting = false;

            OnStopShooting();
        }

        public void SubscribeOnAimTarget(Action onAimTarget)
        {
            OnAimTarget += onAimTarget;
        }

        public void Unsubscribe(Action onAimTarget)
        {
            OnAimTarget -= onAimTarget;
        }

        protected virtual void OnStartShooting() { }

        protected virtual void OnStopShooting() { }

        protected virtual void OnStartAimingAt(Transform target) { }

        protected virtual void OnStartAimingAt(Vector2 worldPosition) { }

        protected virtual void OnStartAimingAt(float localRotation) { }

        protected virtual void OnStopAiming() { }

        protected virtual void OnShooting() { }

        protected virtual void OnAimingAt(Transform target) { }

        protected virtual void OnAimingAt(Vector2 worldPosition) { }

        protected virtual void OnAimingAt(float localRotation) { }
        
        private void TryAimGun()
        {
            if (_currentAimType == AimType.Transform)
            {
                OnAimingAt(_aimTargetTransform);
            }
            else if (_currentAimType == AimType.Position)
            {
                OnAimingAt(_aimTargetPos);
            }
            else if (_currentAimType == AimType.Angle)
            {
                OnAimingAt(_aimTargetRot);
            }
            else
            {
                return;
            }
        }

        private void TryShoot()
        {
            if (_isShooting)
            {
                OnShooting();
            }
        }

        private enum AimType
        {
            Stop = 0,
            Transform = 1,
            Position = 2,
            Angle = 3,
        }
    }
}
