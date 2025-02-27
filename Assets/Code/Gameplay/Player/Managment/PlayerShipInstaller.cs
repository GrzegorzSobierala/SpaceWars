using Game.Utility;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public class PlayerShipInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Utils.BindGetComponent<Rigidbody2D>(Container, gameObject);
            Utils.BindGetComponent<PlayerMovement2D>(Container, gameObject);
            Utils.BindGetComponent<SpringJoint2D>(Container, gameObject);
            Utils.BindGetComponent<ModuleHandler>(Container, gameObject);
            Utils.BindGetComponent<ModuleFactory>(Container, gameObject);
            Container.Bind<CenterOfMass>().FromComponentInChildren(false).AsSingle();
            Container.Bind<HookCannon>().FromComponentInChildren(false).AsSingle();
            Container.Bind<Hook>().FromComponentInChildren(false).AsSingle();

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