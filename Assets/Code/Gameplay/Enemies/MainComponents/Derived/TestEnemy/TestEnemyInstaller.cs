namespace Game.Room.Enemy
{
    public class TestEnemyInstaller : EnemyInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<EnemyGunBase>().FromComponentInHierarchy().AsSingle();

            base.InstallBindings();
        }
    }
}
