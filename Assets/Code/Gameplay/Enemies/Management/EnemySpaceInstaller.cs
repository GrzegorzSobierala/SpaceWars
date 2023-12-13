using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class EnemySpaceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<EnemyBase>().FromComponentsInHierarchy().AsSingle();
            Container.Bind<EnemyManager>().FromInstance(GetComponent<EnemyManager>()).AsSingle();
        }
    }
}
