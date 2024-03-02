using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class CursorEnemyGuardState : EnemyGuardStateBase
    {
        [Inject] private List<EnemyFieldOfView> _views;
        [Inject] private PatrolController _patrolController;
        [Inject] private EnemyMovementBase _movement;

        protected override void OnEnterState()
        {
            base.OnEnterState();
            Subscribe();
            _movement.SetSpeedModifier(0.5f);
            _movement.SetAngularSpeedModifier(0.5f);
            _patrolController.StartPatroling();
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
