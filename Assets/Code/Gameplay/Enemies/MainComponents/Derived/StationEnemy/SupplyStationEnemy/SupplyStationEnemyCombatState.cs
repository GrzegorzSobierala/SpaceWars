using Game.Management;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class SupplyStationEnemyCombatState : EnemyCombatStateBase
    {
        [Inject] private List<EnemyGunBase> _guns;
        [Inject] private PlayerManager _playerManager;

        protected override void OnEnterState()
        {
            base.OnEnterState();

            foreach (var gun in _guns)
            {
                gun.StartAimingAt(_playerManager.PlayerBody.transform);
                gun.StartShooting();
            }
        }

        protected override void OnExitState()
        {
            base.OnExitState();

            foreach (var gun in _guns)
            {
                gun.StopAiming();
                gun.StopShooting();
            }
        }
    }
}
