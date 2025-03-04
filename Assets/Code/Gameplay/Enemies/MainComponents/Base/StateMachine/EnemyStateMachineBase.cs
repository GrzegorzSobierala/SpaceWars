using System;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public abstract class EnemyStateMachineBase : MonoBehaviour
    {
        [Inject] protected EnemyGuardStateBase _guardState;
        [Inject] protected EnemyCombatStateBase _combatState;
        [Inject] protected EnemyDefeatedStateBase _defeatedState;

        [SerializeField] private EnemyStateType _startState = EnemyStateType.Guard;

        private EnemyStateBase _currentState;

        public EnemyStateBase CurrentState => _currentState;

        public EnemyGuardStateBase GuardState => _guardState;
        public EnemyCombatStateBase CombatState => _combatState;
        public EnemyDefeatedStateBase DefeatedState => _defeatedState;

        protected virtual void Start()
        {
            SetUpStates();
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

        public EnemyStateBase GetState(EnemyStateType switchType)
        {
            return switchType switch
            {
                EnemyStateType.Guard => _guardState,
                EnemyStateType.Combad => _combatState,
                EnemyStateType.Defeated => _defeatedState,
                _ => throw new ArgumentOutOfRangeException(nameof(switchType), switchType, null)
            };
        }

        private void SetUpStates()
        {
            _guardState.gameObject.SetActive(false);
            _combatState.gameObject.SetActive(false);
            _defeatedState.gameObject.SetActive(false);

            _currentState = GetState(_startState);
            _currentState.EnterState();
        }

        private void SwitchState(EnemyStateBase state)
        {
            if (_currentState == state)
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
