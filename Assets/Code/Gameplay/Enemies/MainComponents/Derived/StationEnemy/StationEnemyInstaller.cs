using Game.Utility;

namespace Game.Room.Enemy
{
    public class StationEnemyInstaller : EnemyInstaller
    {
        public override void InstallBindings()
        {
            base.InstallBindings();

            Container.Bind<BasicEnemyGun>().FromComponentInHierarchy().AsSingle();

            Utils.BindComponentsInChildrens<SilosHp>(Container, gameObject, false);
        }
    }
}
