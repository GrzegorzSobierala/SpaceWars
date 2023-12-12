using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class TestEnemyDefeatedState : EnemyDefeatedStateBase
    {
        protected override void OnEnterState()
        {
            Destroy(_enemy.gameObject);
        }

        protected override void OnExitState()
        {
            Debug.LogError($"No OnExitState in {typeof(TestEnemyGuardState)}" );
        }
    }
}
