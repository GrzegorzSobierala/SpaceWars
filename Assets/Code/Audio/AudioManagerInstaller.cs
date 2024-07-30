using Game.Management;
using Game.Player.Ship;
using Game.Room;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.Utility;
using Game.Audio;
using Game.Input.System;

namespace Game
{
    public class AudioManagerInstaller : MonoInstaller<AudioManagerInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<AudioManager>().FromComponentOn(gameObject).AsSingle().NonLazy();
            Container.Bind<FmodEvents>().FromComponentOn(gameObject).AsSingle().NonLazy();
        }
    }
}
