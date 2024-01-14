using Game.Room.Enemy;
using NavMeshPlus.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Room.Enemy
{
    public class CursorEnemyInstaller : EnemyInstaller
    {
        public override void InstallBindings()
        {
            base.InstallBindings();

            Container.Bind<NavMeshAgent>().FromInstance(GetComponent<NavMeshAgent>()).AsSingle();
        }
    }
}
