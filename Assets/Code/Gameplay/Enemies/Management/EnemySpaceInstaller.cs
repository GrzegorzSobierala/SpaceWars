using Zenject;

namespace Game.Room.Enemy
{
    public class EnemySpaceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<EnemyBase>().FromComponentsInHierarchy(null, false).AsSingle();
            Container.Bind<EnemyManager>().FromInstance(GetComponent<EnemyManager>()).AsSingle();
        }
    }
}
