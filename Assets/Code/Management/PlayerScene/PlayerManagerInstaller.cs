using Game.Management;
using Game.Player.Ship;
using Game.Room;
using Zenject;

namespace Game.Player
{
    public class PlayerManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<PlayerManager>().FromComponentInHierarchy(false).AsSingle();
            Container.Bind<PlayerSceneManager>().FromComponentInHierarchy(false).AsSingle();
            Container.Bind<GunManager>().FromComponentInHierarchy(false).AsSingle();
        }
    }
}