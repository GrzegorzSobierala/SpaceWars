using Game.Combat;
using Game.Management;
using Game.Physics;
using Game.Player.Ship;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public abstract class EnemyGuardStateBase : EnemyStateBase, IHookedCallBack
    {
        [Inject] protected List<DamageHandlerBase> _damageHandlers;
        [Inject] protected PlayerManager _playerManager;
        [Inject] protected EnemyStateMachineBase _stateMachine;
        [Inject] protected AlarmActivatorTimer _alarmActivatorTimer;

        protected virtual void OnDestroy()
        {
            foreach (var handler in _damageHandlers)
            {
                handler.Unsubscribe(TrySwitchToCombatState);
            }
        }

        public void OnHooked()
        {
            _stateMachine.SwitchToCombatState();
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

        protected void SubscribeToFOVs(List<FieldOfViewEntity> views)
        {
            foreach (var view in views)
            {
                view.OnKnowWherePlayerIs += OnPlayerFind;
            }
        }

        protected void UnubscribeToFOVs(List<FieldOfViewEntity> views)
        {
            foreach (var view in views)
            {
                view.OnKnowWherePlayerIs -= OnPlayerFind;
            }
        }

        private void OnPlayerFind()
        {
            _stateMachine.SwitchToCombatState();
        }

        private void TrySwitchToCombatState(DamageData damage)
        {
            if (_playerManager.PlayerBody.gameObject != damage.DamageDealer)
                return;

            _stateMachine.SwitchToCombatState();
        }
    }
}
