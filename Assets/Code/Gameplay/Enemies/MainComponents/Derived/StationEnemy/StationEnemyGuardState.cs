using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class StationEnemyGuardState : EnemyGuardStateBase
    {
        [Inject] private EnemyBase enemyBase;

        protected override void OnEnterState()
        {
            base.OnEnterState();

            foreach (var gun in enemyBase.GetComponentsInChildren<BasicEnemyGun>())
            {
                gun.StartAimingAt(_playerManager.PlayerBody.transform);
                gun.StartShooting();
            }
        }
    }
}
