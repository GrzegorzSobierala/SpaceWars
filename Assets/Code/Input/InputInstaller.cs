using UnityEngine;
using Zenject;

namespace Game.Input.System
{
    public class InputInstaller : MonoInstaller<InputInstaller>
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.Bind<InputProvider>().FromComponentOn(gameObject).
                AsSingle().NonLazy();
        }
    }
}
