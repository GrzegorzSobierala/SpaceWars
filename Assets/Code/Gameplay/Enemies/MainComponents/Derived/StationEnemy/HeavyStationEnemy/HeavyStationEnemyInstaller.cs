using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class HeavyStationEnemyInstaller : StationEnemyInstaller
    {
        public override void InstallBindings()
        {
            base.InstallBindings();

            Container.Bind<AmmoDepot>().FromComponentInHierarchy().AsSingle();
        }
    }
}
