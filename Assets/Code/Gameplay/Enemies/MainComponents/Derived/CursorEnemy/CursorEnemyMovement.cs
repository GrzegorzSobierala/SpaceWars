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

        private bool _isFallowing = false;

        private float _nextStop = 0;
        private float _stopTime = 0;
        private float _currentStopTime = 0;

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

            if(_nextStop > Time.time)
            {
                _nextStop = Random.Range(10f, 30f) + Time.time;
                _stopTime = Random.Range(1f, 9f);
                _currentStopTime = Time.time;
                _agent.isStopped = true;
            }

            if(_agent.isStopped && _currentStopTime + _stopTime > Time.time)
            {
                _agent.isStopped = false;
            }

            if(Time.frameCount % 25 == 0)
            {
                _agent.SetDestination(fallowTarget.transform.position);
            }

            if (_agent.isStopped)
            {
                RotateByVelocity();
            }
            else
            {
                RotateToPosition(fallowTarget.position);
            }
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

            if (Time.frameCount % 25 == 0)
            {
                _agent.SetDestination(targetPosition);
            }

            if (Vector2.Distance(_body.position, targetPosition) < 50)
            {
                OnAchivedTarget?.Invoke();
            }

            if(_agent.isStopped)
            {
                RotateByVelocity();
            }
            else
            {
                RotateToPosition(targetPosition);
            }
        }

        public override void SetSpeedModifier(float modifier)
        {
            base.SetSpeedModifier(modifier);

            _agent.speed = CurrentSpeed;
        }

        public override void SetAngularSpeedModifier(float modifier)
        {
            base.SetAngularSpeedModifier(modifier);

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

        private void RotateByVelocity()
        {
            float targetAngle = Utils.AngleDirected(_agent.velocity);
            RotateToAngle(targetAngle);
        }

        private void RotateToPosition(Vector2 position)
        {
            float targetAngle = Utils.AngleDirected(_body.position, position);
            RotateToAngle(targetAngle);
        }

        private void RotateToAngle(float angle)
        {
            angle -= 90;
            float rotSpeed = _agent.angularSpeed * Time.fixedDeltaTime;
            float newAngle = Mathf.MoveTowardsAngle(_body.rotation, angle, rotSpeed);

            _body.MoveRotation(newAngle);
        }
    }
}
