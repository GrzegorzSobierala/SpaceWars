using Game.Utility;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Game.Room.Enemy
{
    public class CursorEnemyMovement : EnemyMovementBase
    {
        public override bool UseFixedUpdate => true;

        [Inject] private NavMeshAgent _agent;

        [SerializeField] private float _agentTargetUpdateInterval = 0.1f;

        private float _nextAgentTargetUpdateTime = 0;

        private void Start()
        {
            NavBodySetUp();
        }

        protected override void OnGoingTo(Transform fallowTarget)
        {
            base.OnGoingTo(fallowTarget);

            TrySetAgentDestinationInterval(fallowTarget.position);

            _body.MovePosition(_agent.nextPosition);

            UpdateRotation(fallowTarget.position);
        }

        protected override void OnStartGoingTo(Vector2 targetPosition)
        {
            base.OnStartGoingTo(targetPosition);

            _agent.isStopped = false;

            SetPathToPosition(targetPosition);
        }

        protected override void OnGoingTo(Vector2 targetPosition)
        {
            base.OnGoingTo(targetPosition);

            if (_agent.pathPending || !_agent.hasPath)
            {
                return;
            }

            _body.MovePosition(_agent.nextPosition);

            if (_agent.remainingDistance < _agent.stoppingDistance)
            {
                OnAchivedTarget?.Invoke();
            }

            UpdateRotation(targetPosition);
        }

        public override void SetSpeedModifier(float modifier)
        {
            base.SetSpeedModifier(modifier);

            _agent.speed = CurrentSpeed;
        }

        private void NavBodySetUp()
        {
            _agent.speed = CurrentSpeed;
            _agent.angularSpeed = CurrentAngularSpeed;

            _agent.updatePosition = false;
            _agent.updateRotation = false;

            if (_body.bodyType != RigidbodyType2D.Kinematic)
            {
                Debug.LogError("Rigidbody2D is not Kinematic, changing to dynamic", this);
                _body.bodyType = RigidbodyType2D.Kinematic;
            }

            if (_body.interpolation != RigidbodyInterpolation2D.Interpolate)
            {
                Debug.LogError("Rigidbody2D interpolation is not Interpolate, changing to Interpolate", this);
                _body.interpolation = RigidbodyInterpolation2D.Interpolate;
            }
        }

        private void UpdateRotation(Vector2 targetPosition)
        {
            float targetAngle;
            if (_agent.hasPath && _agent.remainingDistance > _agent.stoppingDistance)
            {
                targetAngle = Utils.AngleDirected(_agent.desiredVelocity);
            }
            else
            {
                targetAngle = Utils.AngleDirected(_body.position, targetPosition);
            }

            RotateToAngle(targetAngle);
        }   

        private void RotateToAngle(float angle)
        {
            float rotSpeed = CurrentAngularSpeed * DeltaTime;
            float newAngle = Mathf.MoveTowardsAngle(_body.rotation, angle, rotSpeed);

            _body.MoveRotation(newAngle);
        }

        private void TrySetAgentDestinationInterval(Vector2 targetPos)
        {
            if (Time.time >= _nextAgentTargetUpdateTime)
            {
                _agent.SetDestination(targetPos);
                _nextAgentTargetUpdateTime = Time.time + _agentTargetUpdateInterval;
            }
        }

        private void SetPathToPosition(Vector2 targetPos)
        {
            if (!_agent.SetDestination(targetPos))
            {
                Debug.LogError("SetDestination failed", this);
            }
        }
    }
}
