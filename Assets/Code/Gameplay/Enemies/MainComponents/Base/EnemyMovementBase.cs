using Game.Testing;
using System;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public abstract class EnemyMovementBase : MonoBehaviour
    {
        public abstract bool UseFixedUpdate { get; }

        public float CurrentSpeed => _baseSpeed * _speedModifier * _test.EnemySpeedMulti;
        public float CurrentAngularSpeed => _baseAngularSpeed * _angularSpeedModifier;
        public float DeltaTime => UseFixedUpdate ? Time.fixedDeltaTime : Time.deltaTime;
        public float BaseSpeed => _baseSpeed;
        public float BaseSpeedForce => _baseSpeedForce;
        public float BaseAngularSpeed => _baseAngularSpeed;
        public float CurrentSpeedModifier => _speedModifier;
        public float CurrentAngularSpeedModifier => _angularSpeedModifier;

        protected Action OnAchivedTarget;
        protected Action OnChangedTarget;

        [Inject] protected Rigidbody2D _body;
        [Inject] protected TestingSettings _test;

        [SerializeField] private float _baseSpeed;
        [SerializeField] private float _baseAngularSpeed;

        private float _baseSpeedForce;
        private MovementType _currentMovementType = MovementType.Stop;
        private Transform _currentTargetTransform;
        private Vector2 _currentTargetPosition;
        private float _speedModifier = 1;
        private float _angularSpeedModifier = 1;

        protected virtual void Awake()
        {
            _baseSpeedForce = _baseSpeed * _body.mass;
        }

        protected virtual void FixedUpdate()
        {
            if (!UseFixedUpdate)
                return;

            UpdateMovement();
        }

        protected virtual void Update()
        {
            if (UseFixedUpdate)
                return;

            UpdateMovement();
        }

        public void StartGoingTo(Vector2 targetPosition)
        {
            _currentMovementType = MovementType.GoingToPosition;
            _currentTargetPosition = targetPosition;
            OnChangedTarget?.Invoke();

            OnStartGoingTo(targetPosition);
        }

        public void StartGoingTo(Transform fallowTarget)
        {
            _currentMovementType = MovementType.GoingToTransform;
            _currentTargetTransform = fallowTarget;
            OnChangedTarget?.Invoke();

            OnStartGoingTo(fallowTarget);
        }

        public void StartRotatingTowards(Vector2 targetPosition)
        {
            _currentMovementType = MovementType.RotatingTowardsPosition;
            _currentTargetPosition = targetPosition;
            OnChangedTarget?.Invoke();

            OnStartRotatingTowards(targetPosition);
        }

        public void StartRotatingTowards(Transform towardsTarget)
        {
            _currentMovementType = MovementType.RotatingTowardsTransform;
            _currentTargetTransform = towardsTarget;
            OnChangedTarget?.Invoke();

            OnStartRotatingTowards(towardsTarget);
        }

        public void StopMoving()
        {
            _currentMovementType = MovementType.Stop;
            OnChangedTarget?.Invoke();

            OnStopMoving();
        }

        public void SubscribeOnAchivedTarget(Action action)
        {
            OnAchivedTarget += action;
        }

        public void UnsubscribeOnAchivedTarget(Action action)
        {
            OnAchivedTarget -= action;
        }

        public void SubscribeOnChangedTarget(Action action)
        {
            OnChangedTarget += action;
        }

        public void UnsubscribeOnChangedTarget(Action action)
        {
            OnChangedTarget -= action;
        }

        public virtual void SetSpeedModifier(float modifier)
        {
            _speedModifier = modifier;
        }

        public virtual void SetAngularSpeedModifier(float modifier)
        {
            _angularSpeedModifier = modifier;
        }

        protected virtual void OnStartGoingTo(Vector2 targetPosition) {}

        protected virtual void OnStartGoingTo(Transform fallowTarget) {}

        protected virtual void OnStartRotatingTowards(Vector2 targetPosition) {}

        protected virtual void OnStartRotatingTowards(Transform towardsTarget) {}

        protected virtual void OnStopMoving() {}

        protected virtual void OnGoingTo(Vector2 targetPosition) { }

        protected virtual void OnGoingTo(Transform fallowTarget) { }

        protected virtual void OnRotatingTowards(Vector2 targetPosition) { }

        protected virtual void OnRotatingTowards(Transform towardsTarget) { }

        private void UpdateMovement()
        {
            switch (_currentMovementType)
            {
                case MovementType.GoingToTransform:
                    OnGoingTo(_currentTargetTransform);
                    break;
                case MovementType.GoingToPosition:
                    OnGoingTo(_currentTargetPosition);
                    break;
                case MovementType.RotatingTowardsTransform:
                    OnRotatingTowards(_currentTargetTransform);
                    break;
                case MovementType.RotatingTowardsPosition:
                    OnRotatingTowards(_currentTargetPosition);
                    break;
            }
        }

        public enum MovementType
        {
            Stop = 0,
            GoingToTransform = 1,
            GoingToPosition = 2,
            RotatingTowardsTransform = 3,
            RotatingTowardsPosition = 4,
        }
    }

    
}
