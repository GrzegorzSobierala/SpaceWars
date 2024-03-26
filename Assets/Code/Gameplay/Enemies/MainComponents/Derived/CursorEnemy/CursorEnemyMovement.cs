using Game.Utility;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Game.Room.Enemy
{
    public class CursorEnemyMovement : EnemyMovementBase
    {
        public override bool UseFixedUpdate => false;

        [Inject] private NavMeshAgent _agent;

        [SerializeField] private float _spotRange = 750;
        [SerializeField] private float _agentTargetUpdateInterval = 0.1f;

        private bool _isFallowing = false;
        private float _nextAgentTargetUpdateTime = 0;

        private void Start()
        {
            _agent.speed = CurrentSpeed;
            _agent.angularSpeed = CurrentAngularSpeed;
        }

        protected override void OnGoingTo(Transform fallowTarget)
        {
            base.OnGoingTo(fallowTarget);

            if(!_isFallowing)
            {
                if (Vector2.Distance(transform.position, fallowTarget.position) < _spotRange)
                {
                    _agent.isStopped = false;
                    _isFallowing = true;
                }
                else
                {
                    return;
                }
            }

            TrySetAgentDestination(fallowTarget.position);

            UpdateRotation(fallowTarget.position);
        }

        protected override void OnStartGoingTo(Vector2 targetPosition)
        {
            base.OnStartGoingTo(targetPosition);

            _agent.isStopped = false;
            _agent.SetDestination(targetPosition);
        }

        protected override void OnGoingTo(Vector2 targetPosition)
        {
            base.OnGoingTo(targetPosition);

            TrySetAgentDestination(targetPosition);

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

        private float CalculatePathLength(NavMeshPath path, Transform target)
        {
            Vector3[] pathCorners = new Vector3[path.corners.Length + 2];

            // Get the corners of the path
            pathCorners[0] = transform.position;
            pathCorners[pathCorners.Length - 1] = target.position;
            path.GetCornersNonAlloc(pathCorners);

            float length = 0f;

            // Sum the distances between consecutive corners
            for (int i = 0; i < pathCorners.Length - 1; i++)
            {
                length += Vector3.Distance(pathCorners[i], pathCorners[i + 1]);
            }

            return length;
        }

        private void UpdateRotation(Vector2 targetPosition)
        {
            float targetAngle = Utils.AngleDirected(_agent.velocity);
            if (_agent.remainingDistance > _agent.stoppingDistance)
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
            angle -= 90;
            float rotSpeed = CurrentAngularSpeed * DeltaTime;
            float newAngle = Mathf.MoveTowardsAngle(_body.rotation, angle, rotSpeed);

            Vector3 newRot = transform.localRotation.eulerAngles;
            newRot.z = newAngle;
            transform.localRotation = Quaternion.Euler(newRot);
        }

        private void TrySetAgentDestination(Vector2 targetPos)
        {
            if (Time.time >= _nextAgentTargetUpdateTime)
            {
                _agent.SetDestination(targetPos);
                _nextAgentTargetUpdateTime = Time.time + _agentTargetUpdateInterval;
            }
        }
    }
}
