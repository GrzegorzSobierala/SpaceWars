using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public abstract class EnemyStateMachineBase : MonoBehaviour
    {
        [Inject] protected EnemyGuardStateBase _guardState;
        [Inject] protected EnemyCombatStateBase _combatState;
        [Inject] protected EnemyDefeatedStateBase _defeatedState;

        private EnemyStateBase _currentState;

        protected virtual void Start()
        {
            Init();
        }

        public void SwitchToGuardState()
        {
            SwitchState(_guardState);
        }

        public void SwitchToCombatState()
        {
            SwitchState(_combatState);
        }

        public void SwitchToDefeatedState()
        {
            SwitchState(_defeatedState);
        }

        private void Init()
        {
            _guardState.EnterState();
            _currentState = _guardState;
        }

        private void SwitchState(EnemyStateBase state)
        {
            if (_currentState != state)
            {
                Debug.Log($"Current state is the same as new : {nameof(state)}");
                return;
            }

            _currentState.ExitState();
            state.EnterState();
            _currentState = state;
        }
    }
}
