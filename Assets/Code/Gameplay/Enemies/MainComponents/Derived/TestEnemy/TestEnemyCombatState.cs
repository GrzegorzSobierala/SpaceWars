using Game.Management;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class TestEnemyCombatState : EnemyCombatStateBase
    {
        [Inject] private EnemyGunBase _gun;
        [Inject] private EnemyMovementBase _movement;
        [Inject] private PlayerManager _playerManager;

        [SerializeField] private float _spotPlayerRange = 750;

        protected override void OnEnterState()
        {
            base.OnEnterState();

            _gun.StartAimingAt(_playerManager.PlayerBody.transform);
            _gun.StartShooting();
            StartCoroutine(TryFallowPlayer());
        }

        protected override void OnExitState()
        {
            base.OnExitState();

            _gun.StopShooting();
            _gun.StopAiming();
            _movement.StopMoving();
        }

        private IEnumerator TryFallowPlayer()
        {
            yield return new WaitUntil(IsPlayerInSpotRange);

            _movement.StartGoingTo(_playerManager.PlayerBody.transform);
        }

        private bool IsPlayerInSpotRange()
        {
            Vector2 playerPos = _playerManager.PlayerBody.position;
            return Vector2.Distance(transform.position, playerPos) < _spotPlayerRange;
        }
    }
}
