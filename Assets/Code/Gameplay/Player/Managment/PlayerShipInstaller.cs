using UnityEngine;
using Zenject;

namespace Game.Player
{
    public class PlayerShipInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<Rigidbody2D>().FromComponentOn(gameObject).AsSingle();
            Container.Bind<PlayerMovement2D>().FromComponentOn(gameObject).AsSingle();
            Container.Bind<PlayerModuleHandler>().FromComponentOn(gameObject).AsSingle();

            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<PlayerCollisionEnter2DSignal>().OptionalSubscriber();
            Container.DeclareSignal<PlayerCollisionStay2DSignal>().OptionalSubscriber();
            Container.DeclareSignal<PlayerCollisionExit2DSignal>().OptionalSubscriber();
            Container.DeclareSignal<PlayerTriggerEnter2DSignal>().OptionalSubscriber();
            Container.DeclareSignal<PlayerTriggerStay2DSignal>().OptionalSubscriber();
            Container.DeclareSignal<PlayerTriggerExit2DSignal>().OptionalSubscriber();
        }
    }
}