using Game.Management;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class StationEnemyStateMachine : EnemyStateMachineBase
    {
        [Inject] BasicEnemyGun _gun;
        [Inject] PlayerManager _playerManager;

        protected override void Start()
        {
            base.Start();
        }
    }
}
