using Game.Testing;
using Game.Utility;
using Game.Utility.Globals;
using NaughtyAttributes;
using System;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Game.Room.Enemy
{
    public abstract class EnemyGunBase : MonoBehaviour
    {
        [Inject] protected Rigidbody2D _body;
        [Inject] private TestingSettings testingSettings;

        [SerializeField] protected UnityEvent OnShoot;
        [SerializeField, AutoFill, Required, AllowNesting] protected Transform _rotationTrans;

        protected Action OnAimTarget;

        protected bool _isAimedAtPlayer = false;

        private AimType _currentAimType = AimType.Stop;
        private bool _isShooting = false;

        private Transform _aimTargetTransform;
        private Vector2 _aimTargetPos;
        private float _aimTargetRot;
        private float _lastTargetAimableTime = -100;
        private float _startAngle;

        private Vector3 _startLocalUpDir;
        private float _startLocalRot;

        protected bool IsAimedAtPlayer => _isAimedAtPlayer;

        protected virtual void Awake()
        {
            Init();
        }

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

        protected virtual void OnStopAiming() 
        {
            _isAimedAtPlayer = false;
        }

        protected virtual void OnShooting() { }

        protected virtual void OnAimingAt(Transform target) { }

        protected virtual void OnAimingAt(Vector2 worldPosition) { }

        protected virtual void OnAimingAt(float localRotation) { }

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

        protected void Aim(Vector2 pos, float travers, float rotateSpeed, 
            float aimedAngle , bool isAimingPlayer)
        {
            float currentAngle = Mathf.MoveTowardsAngle(_rotationTrans.localEulerAngles.z,
                _rotationTrans.localEulerAngles.z + _startLocalRot, float.MaxValue);

            Vector2 targetPosInLocal = _rotationTrans.InverseTransformPoint(pos);

            targetPosInLocal = Utils.RotateVector(targetPosInLocal, _startLocalRot);

            float angleToTarget = Vector2.SignedAngle(Vector2.up, targetPosInLocal.normalized);

            float targetAngle = Mathf.MoveTowardsAngle(currentAngle, currentAngle + angleToTarget, float.MaxValue);

            targetAngle = ConvertAngleToNegative180To180(targetAngle);

            float targetAngleClamped = Mathf.Clamp(targetAngle, -(travers / 2), 
                (travers / 2));
            float maxDelta = rotateSpeed * Time.deltaTime;


            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngleClamped, maxDelta);

            newAngle = Mathf.MoveTowardsAngle(newAngle, newAngle - _startLocalRot, float.MaxValue);

            _rotationTrans.localRotation = Quaternion.Euler(0, 0, newAngle);

            Vector2 newVectorToTarget = pos - (Vector2)_rotationTrans.position;
            float newAngleToTarget = Vector2.SignedAngle(_rotationTrans.up, newVectorToTarget);

            if (newAngleToTarget >= -aimedAngle / 2 && newAngleToTarget <= aimedAngle / 2)
            {
                OnAimTarget?.Invoke();
                _isAimedAtPlayer = isAimingPlayer;
            }
            else
            {
                _isAimedAtPlayer = false;
            }
        }

        float ConvertAngleToNegative180To180(float angle)
        {
            return (angle > 180) ? angle - 360 : angle;
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
            _startLocalUpDir = _rotationTrans.localRotation * Vector3.up;
            _startLocalRot = _rotationTrans.localEulerAngles.z;
        }

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
            if (!testingSettings.EnableEnemyShooting)
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
    }
}
