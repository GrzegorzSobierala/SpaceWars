using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class CargoEnemyGuardState : EnemyGuardStateBase
    {
        [Inject] private List<EnemyFieldOfView> _views;

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
                view.OnTargetFound += OnPlayerFind;
            }
        }

        private void Unubscribe()
        {
            foreach (var view in _views)
            {
                view.OnTargetFound -= OnPlayerFind;
            }
        }

        private void OnPlayerFind(GameObject foundTarget)
        {
            _stateMachine.SwitchToCombatState();
        }
    }
}
