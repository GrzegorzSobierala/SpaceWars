using Game.Physics;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class CargoEnemyGuardState : EnemyGuardStateBase
    {
        [Inject] private List<FieldOfViewEntity> _views;

        protected override void OnEnterState()
        {
            base.OnEnterState();
            Subscribe();
        }

        protected override void OnExitState()
        {
            base.OnExitState();
            Unubscribe();
        }

        private void Subscribe()
        {
            foreach (var view in _views)
            {
                view.OnKnowWherePlayerIs += OnPlayerFind;
            }
        }

        private void Unubscribe()
        {
            foreach (var view in _views)
            {
                view.OnKnowWherePlayerIs -= OnPlayerFind;
            }
        }

        private void OnPlayerFind()
        {
            _stateMachine.SwitchToCombatState();
        }
    }
}
