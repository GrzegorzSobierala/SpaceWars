using Zenject;

namespace Game.Audio
{
    public class AudioManagerInstaller : MonoInstaller<AudioManagerInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<AudioManager>().FromComponentOn(gameObject).AsSingle().NonLazy();
            Container.Bind<BackgroundMusicManager>().FromComponentOn(gameObject).AsSingle().NonLazy();
            Container.Bind<FMODEvents>().FromComponentOn(gameObject).AsSingle().NonLazy();
        }
    }
}