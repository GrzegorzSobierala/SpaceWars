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
        [Inject] private EnemyGunBase _enemyGun;

        [SerializeField] private float _movementSpeedMulti = 0.5f;

        protected override void OnEnterState()
        {
            base.OnEnterState();
            SubscribeToFOVs(_views);
            _movement.SetSpeedModifier(_movementSpeedMulti);
            _movement.SetAngularSpeedModifier(_movementSpeedMulti);
            _patrolController.StartPatroling();
        }

        protected override void OnExitState()
        {
            base.OnExitState();
            UnubscribeToFOVs(_views);
            _enemyGun.Prepare();
        }
    }
}
