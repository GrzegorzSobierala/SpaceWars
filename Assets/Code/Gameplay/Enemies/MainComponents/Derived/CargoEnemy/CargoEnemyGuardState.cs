using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class CargoEnemyGuardState : EnemyGuardStateBase
    {
        [Inject] private List<EnemyFieldOfView> _views;
        [Inject] private EnemyMovementBase _movement;

        protected override void OnEnterState()
        {
            base.OnEnterState();
            Subscribe();
            _movement.StartGoingTo(new Vector2(12, 1569));
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
