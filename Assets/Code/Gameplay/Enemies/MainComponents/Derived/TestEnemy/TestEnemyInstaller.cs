using Game.Room.Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class TestEnemyInstaller : EnemyInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<EnemyGunBase>().FromComponentInHierarchy().AsSingle();

            base.InstallBindings();
        }
    }
}
