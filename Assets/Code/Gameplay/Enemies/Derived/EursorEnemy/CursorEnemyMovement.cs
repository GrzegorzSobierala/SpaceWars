using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class CursorEnemyMovement : EnemyMovementBase
    {
        public override bool UseFixedUpdate => true;

        //[SerializeField] private ;

        protected override void OnGoingTo(Transform fallowTarget)
        {
            base.OnGoingTo(fallowTarget);
        }
    }
}
