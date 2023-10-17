using Game.Player.Modules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game
{
    public class PlayerHullInstaler : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<PlayerHullBase>().FromInstance(GetComponent<PlayerHullBase>()).AsSingle();
        }
    }
}
