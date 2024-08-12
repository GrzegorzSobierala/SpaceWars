using Zenject;
using Game.Audio;

namespace Game
{
    public class AudioManagerInstaller : MonoInstaller<AudioManagerInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<AudioManager>().FromComponentOn(gameObject).AsSingle().NonLazy();
            Container.Bind<FMODEvents>().FromComponentOn(gameObject).AsSingle().NonLazy();
            Container.Bind<FMODBuses>().FromComponentOn(gameObject).AsSingle().NonLazy();
        }
    }
}