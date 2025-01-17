using Cinemachine;
using UnityEngine;
using Zenject;

namespace Game.Player.Control
{
    public class VirtualCameraInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<CinemachineVirtualCamera>().FromComponentOn(gameObject).AsSingle();
            Container.Bind<VirtualCameraController>().FromComponentOn(gameObject).AsSingle();
        }
    }
}