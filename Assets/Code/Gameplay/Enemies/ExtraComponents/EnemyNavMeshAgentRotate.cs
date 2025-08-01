using Game.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Game.Room.Enemy
{
    public class EnemyNavMeshAgentRotate : MonoBehaviour
    {
        [Inject] private NavMeshAgent _agent;
        [Inject] private Rigidbody2D _body;

        private void FixedUpdate()
        {
            MoveRotationToVelocity();
        }

        private void MoveRotationToVelocity()
        {
            Vector2 velocity = _agent.velocity;

            float targetAngle = Utils.AngleDirected(velocity);

            RotateToAngle(targetAngle);
        }

        public void RotateToAngle(float angle)
        {
            float rotSpeed = _agent.angularSpeed * Time.fixedDeltaTime;
            float newAngle = Mathf.MoveTowardsAngle(_body.rotation, angle, rotSpeed);

            _body.MoveRotation(newAngle);
        }
    }
}
