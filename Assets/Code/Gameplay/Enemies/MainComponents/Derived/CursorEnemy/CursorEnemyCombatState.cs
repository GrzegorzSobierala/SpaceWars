using Game.Management;
using Zenject;

namespace Game.Room.Enemy
{
    public class CursorEnemyCombatState : EnemyCombatStateBase
    {
        [Inject] private EnemyGunBase _gun;
        [Inject] private EnemyMovementBase _movement;
        [Inject] private PlayerManager _playerManager;

        protected override void OnEnterState()
        {
            base.OnEnterState();

            _gun.StartAimingAt(_playerManager.PlayerBody.transform);
            _gun.StartShooting();
            _movement.SetSpeedModifier(1.0f);
            _movement.SetAngularSpeedModifier(1.0f);
            _movement.StartGoingTo(_playerManager.PlayerBody.transform);
        }

        protected override void OnExitState()
        {
            base.OnExitState();

            _gun.StopShooting();
            _gun.StopAiming();
            _movement.StopMoving();
        }
    }
}
