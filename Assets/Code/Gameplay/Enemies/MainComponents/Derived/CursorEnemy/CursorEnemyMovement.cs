using NavMeshPlus.Extensions;
using System.Collections;
using System.Collections.Generic;
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
    }
}
