using UnityEngine;

namespace Game.Room.Enemy
{
    public class CargoEnemyMovement : EnemyMovementBase
    {
        public override bool UseFixedUpdate => false;

        protected override void OnGoingTo(Vector2 targetPosition)
        {
            base.OnGoingTo(targetPosition);
        }
    }
}
