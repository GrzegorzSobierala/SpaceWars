using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public abstract class EnemyMovementBase : MonoBehaviour
    {
        public abstract bool UseFixedUpdate { get; }

        public float CurrentSpeedModifier = 1;

        protected Action OnAchivedTarget;

        protected float BaseSpeed => _baseSpeed;
        protected float BaseSpeedForece => _baseSpeedForece;

        [Inject] protected Rigidbody2D _body;

        [SerializeField] private float _baseSpeed;

        private float _baseSpeedForece;
        private MovementType _currentMovementType = MovementType.Stop;
        private Transform _currentTargetTransform;
        private Vector2 _currentTargetPosition;


        protected virtual void Awake()
        {
            _baseSpeedForece = _baseSpeed * _body.mass;
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

            OnStartGoingTo(targetPosition);
        }

        public void StartGoingTo(Transform fallowTarget)
        {
            _currentMovementType = MovementType.GoingToTransform;
            _currentTargetTransform = fallowTarget;

            OnStartGoingTo(fallowTarget);
        }

        public void StartRotatingTowards(Vector2 targetPosition)
        {
            _currentMovementType = MovementType.RotatingTowardsPosition;
            _currentTargetPosition = targetPosition;

            OnStartRotatingTowards(targetPosition);
        }

        public void StartRotatingTowards(Transform towardsTarget)
        {
            _currentMovementType = MovementType.RotatingTowardsTransform;
            _currentTargetTransform = towardsTarget;

            OnStartRotatingTowards(towardsTarget);
        }

        public void StopMoving()
        {
            _currentMovementType = MovementType.Stop;

            OnStopMoving();
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
            if(_currentMovementType == MovementType.GoingToTransform)
            {
                OnGoingTo(_currentTargetTransform);
            }
            else if (_currentMovementType == MovementType.GoingToPosition)
            {
                OnGoingTo(_currentTargetPosition);
            }
            else if (_currentMovementType == MovementType.RotatingTowardsTransform)
            {
                OnRotatingTowards(_currentTargetTransform);
            }
            else if (_currentMovementType == MovementType.RotatingTowardsPosition)
            {
                OnRotatingTowards(_currentTargetPosition);
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
