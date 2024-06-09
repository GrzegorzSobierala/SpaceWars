using Zenject;

namespace Game.Input.System
{
    public class InputInstaller : MonoInstaller<InputInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<InputProvider>().FromComponentOn(gameObject).AsSingle().NonLazy();
        }
    }
}
