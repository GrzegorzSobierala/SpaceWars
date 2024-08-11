using Game.Utility;
using UnityEngine.AI;

namespace Game.Room.Enemy
{
    public class CursorEnemyInstaller : EnemyInstaller
    {
        public override void InstallBindings()
        {
            base.InstallBindings();

            Utils.BindGetComponent<NavMeshAgent>(Container, gameObject);

            Container.Bind<PatrolController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<EnemyGunBase>().FromComponentInHierarchy().AsSingle();
            Container.Bind<CursorEnemyGun>().FromComponentInHierarchy().AsSingle();
        }
    }
}
