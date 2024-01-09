using Game.Combat;
using UnityEditorInternal;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class CursorEnemyGuardState : EnemyGuardStateBase
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
