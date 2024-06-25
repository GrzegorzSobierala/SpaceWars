using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class TestEnemyGuardState : EnemyGuardStateBase
    {
        [Inject] private List<EnemyFieldOfView> _views;
        [Inject] private EnemyGunBase _enemyGun;
        [Inject] private EnemyMovementBase _enemyMovement;

        [SerializeField] private bool _startMovementOnEnter = false;

        protected override void OnEnterState()
        {
            base.OnEnterState();
            Subscribe();
            if(_startMovementOnEnter)
            {
                _enemyMovement.StartGoingTo(_playerManager.PlayerBody.transform);
            }
        }

        protected override void OnExitState()
        {
            base.OnExitState();
            Unubscribe();
            _enemyGun.Prepare();
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
