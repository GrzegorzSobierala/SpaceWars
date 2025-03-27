using Game.Physics;

namespace Game.Room.Enemy
{
    public class TestEnemyInstaller : EnemyInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<EnemyGunBase>().FromComponentInHierarchy().AsSingle();
            Container.Bind<FieldOfViewEntitiesController>().FromComponentInHierarchy(false).AsSingle().NonLazy();

            base.InstallBindings();
        }
    }
}
