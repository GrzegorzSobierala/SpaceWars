using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{

    public class SupplyStationEnemyDefeatedState : EnemyDefeatedStateBase
    {
        [Inject] private EnemyBase _enemy;

        protected override void OnEnterState()
        {
            base.OnEnterState();

            Destroy(_enemy.gameObject);
        }
    }
}
