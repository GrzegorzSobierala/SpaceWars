using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public abstract class EnemyMovementBase : MonoBehaviour
    {
        public float CurrentSpeedModifier = 1;

        protected Action OnAchivedTarget;

        protected float BaseSpeed => _baseSpeed;
        protected float BaseSpeedForece => _baseSpeedForece;

        [Inject] protected Rigidbody2D _body;

        [SerializeField] private float _baseSpeed;

        private float _baseSpeedForece;

        protected virtual void Awake()
        {
            _baseSpeedForece = _baseSpeed * _body.mass;
        }

        public abstract void StartGoingTo(Vector2 targetPosition);

        public abstract void StartGoingTo(Transform fallowTarget);

        public abstract void StartRotatingTowards(Vector2 targetPosition);

        public abstract void StartRotatingTowards(Transform towardsTarget);

        public abstract void StopMoving();
    }
}
