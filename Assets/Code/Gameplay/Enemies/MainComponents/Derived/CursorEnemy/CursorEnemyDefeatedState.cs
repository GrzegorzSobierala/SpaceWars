using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class CursorEnemyDefeatedState : EnemyDefeatedStateBase
    {
        [Inject] private EnemyBase _enemy;

        protected override void OnEnterState()
        {
            Destroy(_enemy.gameObject);
        }

        protected override void OnExitState()
        {
        }
    }
}
