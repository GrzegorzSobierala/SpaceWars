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
        [Inject] private List<EnemyGunBase> _guns;

        protected override void OnEnterState()
        {
            base.OnEnterState();
            //Subscribe();

            foreach (var gun in _guns) 
            {
                gun.StartAimingAt(_playerManager.PlayerBody.transform);
            }
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
