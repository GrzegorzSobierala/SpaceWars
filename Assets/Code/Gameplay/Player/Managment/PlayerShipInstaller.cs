using UnityEngine;
using Zenject;
using Cinemachine;
using Game.Player.Modules;

namespace Game.Player
{
    public class PlayerShipInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<Rigidbody2D>().FromComponentOn(gameObject).AsSingle();

            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<OnPlayerCollision2DEnterSignal>().OptionalSubscriber();
        }
    }
}