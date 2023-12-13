using Game.Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Room.Enemy
{
    public abstract class EnemyGunBase : MonoBehaviour
    {
        public bool IsNotImplementedDebugOn {  get; set; } =  true;

        protected Action OnAimTarget;

        private bool _isShooting = false;
        private bool _isTransformAiming = false;
        private bool _isPosAiming = false;
        private bool _isRotAiming = false;

        private Transform _aimTargetTransform;
        private Vector2 _aimTargetPos;
        private float _aimTargetRot;

        protected virtual void Update()
        {
            TryAimGun();

            TryShoot();
        }

        public void StartAimingAt(Transform target)
        {
            _isTransformAiming = true;

            _isPosAiming = false;
            _isRotAiming = false;

            _aimTargetTransform = target;

            OnStartAimingAt(target);
        }

        public void StartAimingAt(Vector2 worldPosition)
        {
            _isPosAiming = true;

            _isTransformAiming = false;
            _isRotAiming = false;

            _aimTargetPos = worldPosition;

            OnStartAimingAt(worldPosition);
        }

        public void StartAimingAt(float localRotation)
        {
            _isRotAiming = true;

            _isPosAiming = false;
            _isTransformAiming = false;

            _aimTargetRot = localRotation;

            OnStartAimingAt(localRotation);
        }

        public void StopAiming()
        {
            _isRotAiming = false;
            _isPosAiming = false;
            _isTransformAiming = false;

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

        public void SubscribeOnAimTarget(Action onAimTarget)
        {
            OnAimTarget += onAimTarget;
        }

        public void Unsubscribe(Action onAimTarget)
        {
            OnAimTarget -= onAimTarget;
        }

        private void TryAimGun()
        {
            if (_isTransformAiming)
            {
                OnAimingAt(_aimTargetTransform);
            }
            else if (_isPosAiming)
            {
                OnAimingAt(_aimTargetPos);
            }
            else if (_isRotAiming)
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
    }
}
