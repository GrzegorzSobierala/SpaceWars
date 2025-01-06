using System.Collections.Generic;
using Zenject;

namespace Game.Room.Enemy
{
    public class HeavyStationEnemyDefeatedState : EnemyDefeatedStateBase
    {
        [Inject] private EnemyBase _enemyBase;

        protected override void OnEnterState()
        {
            base.OnEnterState();

            Destroy(_enemyBase.gameObject);
        }
    }
}
