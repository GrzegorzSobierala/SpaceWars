using Game.Management;
using System.Collections.Generic;
using Zenject;

namespace Game.Room.Enemy
{
    public class CargoEnemyCombatState : EnemyCombatStateBase
    {
        [Inject] protected PlayerManager _playerManager;
        [Inject] private List<EnemyGunBase> _guns;

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
