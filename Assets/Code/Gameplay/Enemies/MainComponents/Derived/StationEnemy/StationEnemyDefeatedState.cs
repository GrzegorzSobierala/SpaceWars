using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class StationEnemyDefeatedState : EnemyDefeatedStateBase
    {
        [Inject] private EnemyBase _enemyBase;

        protected override void OnEnterState()
        {
            base.OnEnterState();

            Destroy(_enemyBase.gameObject);
        }

    }
}
