using Game.Physics;
using Game.Utility;

namespace Game.Room.Enemy
{
    public class StationEnemyInstaller : EnemyInstaller
    {
        public override void InstallBindings()
        {
            base.InstallBindings();

            Utils.BindComponentsInChildrens<SilosHp>(Container, gameObject, false);
            Utils.BindComponentsInChildrens<EnemyGunBase>(Container, gameObject, false);

            Container.Bind<DockPlace>().FromComponentInHierarchy().AsSingle();
            //Container.Bind<FieldOfViewEntitiesController>().FromComponentInHierarchy(false).AsSingle().NonLazy();

        }
    }
}
