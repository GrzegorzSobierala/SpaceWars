using Zenject;

namespace Game.Room.Enemy
{
    public class TestEnemyGuardState : EnemyGuardStateBase
    {
        [Inject] private EnemyStateMachineBase _stateMachine;

        protected override void OnEnterState()
        {
            _stateMachine.SwitchToCombatState();
        }

        protected override void OnExitState()
        {
        }
    }
}
