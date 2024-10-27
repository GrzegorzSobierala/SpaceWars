using Game.Management;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class StationEnemyStateMachine : EnemyStateMachineBase
    {
        [Inject] BasicEnemyGun gun;
        [Inject] PlayerManager playerManager;

        protected override void Start()
        {
            base.Start();

            gun.StartAimingAt(playerManager.PlayerBody.transform);
        }
    }
}
