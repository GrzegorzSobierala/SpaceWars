using Game.Room.Enemy;
using Game.Utility;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room
{
    public class TestRoomInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Utils.BindGetComponent<RoomManager>(Container, gameObject);
            Container.Bind<PlayerObjectsParent>().FromComponentInHierarchy().AsSingle();
        }
    }
}