using Game.Management;
using Zenject;

namespace Game.Room.Enemy
{
    public class HeavyStationEnemyCombatState : EnemyCombatStateBase
    {
        [Inject] private WeaponsController _weaponController;
        [Inject] protected PlayerManager _playerManager;

        protected override void OnEnterState()
        {
            base.OnEnterState();

            _weaponController.StartShooting();
        }

        protected override void OnExitState()
        {
            base.OnExitState();

            _weaponController.StopShooting();
        }
    }
}
