using Game.Management;
using Zenject;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class CursorEnemyCombatState : EnemyCombatStateBase
    {
        [Inject] private CursorEnemyGun _gun;
        [Inject] private EnemyMovementBase _movement;
        [Inject] private PlayerManager _playerManager;

        protected override void OnEnterState()
        {
            base.OnEnterState();

            _gun.StartShooting();
            _movement.SetSpeedModifier(1.0f);
            _movement.SetAngularSpeedModifier(1.0f);

            FallowPlayer();

            _gun.SubscribeOnStartReload(RunFromPlayer);
            _gun.SubscribeOnStopReload(FallowPlayer);
        }

        protected override void OnExitState()
        {
            base.OnExitState();

            _gun.StopShooting();
            _gun.StopAiming();
            _movement.StopMoving();

            _gun.UnsubscribeOnStartReload(RunFromPlayer);
            _gun.UnsubscribeOnStopReload(FallowPlayer);
        }

        private void FallowPlayer()
        {
            _gun.StartAimingAt(_playerManager.PlayerBody.transform);
            _movement.StartGoingTo(_playerManager.PlayerBody.transform);
        }

        private void RunFromPlayer()
        {
            _gun.StopAiming();
            _movement.StartGoingTo(Vector2.zero);
        }
    }
}
