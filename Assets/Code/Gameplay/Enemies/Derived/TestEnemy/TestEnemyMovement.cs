using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class TestEnemyMovement : EnemyMovementBase
    {
        public override void StartGoingTo(Vector2 targetPosition)
        {
            throw new System.NotImplementedException();
        }

        public override void StartGoingTo(Transform fallowTarget)
        {
            throw new System.NotImplementedException();
        }

        public override void StartRotatingTowards(Vector2 targetPosition)
        {
            throw new System.NotImplementedException();
        }

        public override void StartRotatingTowards(Transform towardsTarget)
        {
            throw new System.NotImplementedException();
        }

        public override void StopMoving()
        {
            throw new System.NotImplementedException();
        }
    }
}
