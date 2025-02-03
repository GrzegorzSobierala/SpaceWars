using Game.Testing;
using Game.Utility;
using Game.Utility.Globals;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Zenject;
using Game.Management;

namespace Game.Room.Enemy
{
    public abstract class EnemyGunBase : MonoBehaviour
    {
        public event Func<bool> CanShoot;

        [Inject] protected Rigidbody2D _body;
        [Inject] private TestingSettings _testingSettings;
        [Inject] private GlobalAssets _globalAssets;

        [SerializeField, AutoFill, Required, AllowNesting] protected Transform _rotationTrans;


        [SerializeField] protected float _beforeShootIndicateTime = 0.25f;
        [SerializeField] private UnityEvent _onBeforeShootGun;
        [SerializeField, FormerlySerializedAs("OnShoot")] protected UnityEvent _onShoot;

        protected Action OnAimTarget;

        protected bool _isAimedAtPlayer = false;

        private AimType _currentAimType = AimType.Stop;
        private bool _isShooting = false;

        private Transform _aimTargetTransform;
        private Vector2 _aimTargetPos;
        private float _aimTargetRot;
        private float _lastTargetAimableTime = -100;

        private float _startLocalRot;

        protected bool IsAimedAtPlayer => _isAimedAtPlayer;
        protected float CurrentGunRot => Utils.GetAngleIn180Format(_rotationTrans.localEulerAngles.z - _startLocalRot);

        protected virtual void Awake()
        {
            Init();
        }

        protected virtual void Update()
        {
            TryUpdateAiming();

            TryUpdateShooting();
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

        public void DefaultBeforeShootAction()
        {
            foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
            {
                List<Material> materials = new();
                renderer.GetSharedMaterials(materials);
                materials.Insert(0, _globalAssets.TestMaterial);
                renderer.SetSharedMaterials(materials);
            }

            Invoke(nameof(RestoreMaterial), _beforeShootIndicateTime / 2);
        }

        private void RestoreMaterial()
        {
            foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
            {
                List<Material> materials = new();
                renderer.GetSharedMaterials(materials);
                materials.RemoveAt(0);
                renderer.SetSharedMaterials(materials);
            }
        }

        protected abstract void OnShoot();

        protected virtual void OnStartShooting() { }

        protected virtual void OnStopShooting() { }

        protected virtual void OnStartAimingAt(Transform target) { }

        protected virtual void OnStartAimingAt(Vector2 worldPosition) { }

        protected virtual void OnStartAimingAt(float localRotation) { }

        protected virtual void OnStopAiming() 
        {
            _isAimedAtPlayer = false;
        }

        protected virtual void OnShooting() { }

        protected virtual void OnAimingAt(Transform target) { }

        protected virtual void OnAimingAt(Vector2 worldPosition) { }

        protected virtual void OnAimingAt(float localRotation) { }

        [Button]
        protected bool TryShoot()
        {
            if (CanShoot?.Invoke() == false)
                return false;

            if(_onBeforeShootGun.GetPersistentEventCount() == 0)
            {
                DefaultBeforeShootAction();
            }
            else
            {
                _onBeforeShootGun.Invoke();
            }

            Invoke(nameof(InvokeShootEvents), _beforeShootIndicateTime);
            return true;
        }

        private void InvokeShootEvents()
        {
            OnShoot();
            _onShoot?.Invoke();
        }

        #region HelperMethods

        protected bool IsTargetVisable(RaycastHit2D[] raycastHits)
        {
            if (raycastHits[0].rigidbody == null)
            {
                return false;
            }

            int layer = LayerMask.NameToLayer(Layers.Player);
            if (raycastHits[0].rigidbody.gameObject.layer == layer)
            {
                return true;
            }

            return false;
        }

        protected bool IsKnowWherePlayerIs(Vector2 targetPos, float range, 
            float noSeeKnowTime, ContactFilter2D contactFilter)
        {
            Vector2 gunPos = _rotationTrans.position;
            Vector2 gunToTargetDir = targetPos - (Vector2)_rotationTrans.position;
            float distanceToTarget = Vector2.Distance(targetPos, gunPos);
            bool isInRange = distanceToTarget < range;

            bool isVisable = isInRange;
            if (isInRange)
            {
                RaycastHit2D[] raycastHits = new RaycastHit2D[1];
                Physics2D.Raycast(gunPos, gunToTargetDir, contactFilter, raycastHits, range);
                isVisable = IsTargetVisable(raycastHits);
            }

            if (isVisable)
            {
                _lastTargetAimableTime = Time.time;
            }

            return _lastTargetAimableTime + noSeeKnowTime > Time.time;
        }

        protected void Aim(Vector2 targetPos, float travers, float rotateSpeed, float aimedAngle 
            , bool isAimingPlayer)
        {
            float currentAngle = _rotationTrans.localEulerAngles.z - _startLocalRot;

            Vector2 targetPosInLocal = _rotationTrans.InverseTransformPoint(targetPos);
            float angleToTarget = Vector2.SignedAngle(Vector2.up, targetPosInLocal);
            float targetAngle = Utils.GetAngleIn180Format(currentAngle + angleToTarget);

            float newAngle = Aim(targetAngle, travers, rotateSpeed);

            float newAngleToTarget = Mathf.DeltaAngle(targetAngle, newAngle);
            CheckAim(newAngleToTarget, aimedAngle, isAimingPlayer);
        }

        protected void Aim(float targetAngle, float travers, float rotateSpeed, float aimedAngle
            , bool isAimingPlayer)
        {
            float newAngle = Aim(targetAngle, travers, rotateSpeed);

            float angleToTarget = Mathf.DeltaAngle(targetAngle, newAngle);

            CheckAim(angleToTarget, aimedAngle, isAimingPlayer);
        }

        private void CheckAim(float angleToTarget, float aimedAngle, bool isAimingPlayer)
        {
            if (angleToTarget >= -aimedAngle / 2 && angleToTarget <= aimedAngle / 2)
            {
                OnAimTarget?.Invoke();
                _isAimedAtPlayer = isAimingPlayer;
            }
            else
            {
                _isAimedAtPlayer = false;
            }
        }

        private float Aim(float targetAngle, float travers, float rotateSpeed)
        {
            float currentAngle = _rotationTrans.localEulerAngles.z - _startLocalRot;
            currentAngle = Utils.GetAngleIn180Format(currentAngle);

            float targetAngleClamped = Mathf.Clamp(targetAngle, -travers / 2, travers / 2);
            float traversSaveTargetAngle = GetTargetAngleToAvoidTravers(currentAngle, targetAngleClamped
                , travers);

            float maxDelta = rotateSpeed * Time.deltaTime;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, traversSaveTargetAngle, maxDelta);
            float newRotAngle = newAngle + _startLocalRot;

            _rotationTrans.localRotation = Quaternion.Euler(0, 0, newRotAngle);
            return newAngle;
        }

        protected void VerticalRotate(Transform toRotate, Transform handle, Vector2 target)
        {
            float distance = Vector2.Distance(handle.position, target);
            Vector2 lookAtPos = handle.up * distance + handle.position;
            toRotate.LookAt(lookAtPos, -handle.forward);
        }

        #endregion

        private void Init()
        {
            _startLocalRot = _rotationTrans.localEulerAngles.z;
        }

        private void TryUpdateAiming()
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

        private void TryUpdateShooting()
        {
            if (!_testingSettings.EnableEnemyShooting)
                return;

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

        private float GetTargetAngleToAvoidTravers(float currentAngle, float targetAngleClamped
            , float travers)
        {
            if (travers >= 360)
                return targetAngleClamped;

            if (currentAngle >= 0 && targetAngleClamped >= 0)
                return targetAngleClamped;

            if (currentAngle < 0 && targetAngleClamped < 0)
                return targetAngleClamped;

            if (Mathf.Abs(currentAngle) + Mathf.Abs(targetAngleClamped) < 180)
                return targetAngleClamped;

            return 0;
        }
    }
}
