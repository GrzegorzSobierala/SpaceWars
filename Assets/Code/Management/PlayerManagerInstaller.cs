using Game.Management;
using Game.Room;
using Zenject;

public class PlayerManagerInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<PlayerManager>().FromComponentInHierarchy(false).AsSingle();
        Container.Bind<TestSceneManager>().FromComponentInHierarchy(false).AsSingle();
    }
}