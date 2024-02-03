using Game.Combat;
using Game.Management;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public abstract class EnemyGuardStateBase : EnemyStateBase
    {
        [Inject] protected List<DamageHandlerBase> _damageHandlers;
        [Inject] protected PlayerManager _playerManager;
        [Inject] protected EnemyStateMachineBase _stateMachine;
        [Inject] protected AlarmActivatorTimer _alarmActivatorTimer;

        public virtual void OnDestroy()
        {
            foreach (var handler in _damageHandlers)
            {
                handler.Unsubscribe(TrySwitchToCombatState);
            }
        }

        protected override void OnEnterState()
        {
            _alarmActivatorTimer.Activate();
            foreach (var handler in _damageHandlers)
            {
                handler.Subscribe(TrySwitchToCombatState);
            }
        }

        protected override void OnExitState()
        {
            foreach (var handler in _damageHandlers)
            {
                handler.Unsubscribe(TrySwitchToCombatState);
            }
        }

        private void TrySwitchToCombatState(Collision2D _ , DamageData damage)
        {
            if (_playerManager.PlayerBody.gameObject != damage.DamageDealer)
                return;

            _stateMachine.SwitchToCombatState();
        }
    }
}
