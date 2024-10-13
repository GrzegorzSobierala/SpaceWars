using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class TestEnemyDefeatedState : EnemyDefeatedStateBase
    {
        [Inject] private EnemyBase _enemy;

        protected override void OnEnterState()
        {
            base.OnEnterState();

            Destroy(_enemy.gameObject);
        }

        protected override void OnExitState()
        {
            Debug.LogError($"No OnExitState in {typeof(TestEnemyGuardState)}" );
        }
    }
}
