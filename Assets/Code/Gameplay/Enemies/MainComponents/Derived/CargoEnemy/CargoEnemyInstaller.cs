using Game.Utility;
using UnityEngine.AI;

namespace Game.Room.Enemy
{
    public class CargoEnemyInstaller : EnemyInstaller
    {
        public override void InstallBindings()
        {
            base.InstallBindings();

            Utils.BindGetComponent<NavMeshAgent>(Container);

            Utils.BindComponentsInChildrens<EnemyGunBase>(Container);
        }
    }
}
