using Zenject;

namespace Game.Room.Enemy
{
    public class TestEnemyGuardState : EnemyGuardStateBase
    {
        protected override void OnEnterState()
        {
            _stateMachine.SwitchToCombatState();
        }

        protected override void OnExitState()
        {
        }
    }
}
