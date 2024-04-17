using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Game.Room.Enemy
{
    public class CargoEnemyMovement : EnemyMovementBase
    {
        public override bool UseFixedUpdate => false;

        [Inject] private NavMeshAgent _agent;

        private void Start()
        {
            _agent.speed = CurrentSpeed;
            _agent.angularSpeed = CurrentAngularSpeed;
        }

        protected override void OnGoingTo(Vector2 targetPosition)
        {
            base.OnGoingTo(targetPosition);
        }
    }
}
