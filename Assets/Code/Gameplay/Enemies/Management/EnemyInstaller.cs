using Game.Combat;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class EnemyInstaller : MonoInstaller<EnemyInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<DamageHandlerBase>().FromComponentsInHierarchy().AsSingle();
            Container.Bind<EnemyFieldOfView>().FromComponentsInHierarchy().AsSingle();

            Container.Bind<EnemyCombatStateBase>().FromComponentInChildren().AsSingle();
            Container.Bind<EnemyDefeatedStateBase>().FromComponentInChildren().AsSingle();
            Container.Bind<EnemyGuardStateBase>().FromComponentInChildren().AsSingle();
            Container.Bind<EnemyStateMachineBase>().FromComponentInChildren().AsSingle();

            Container.Bind<Rigidbody2D>().FromInstance(GetComponent<Rigidbody2D>()).AsSingle();
            Container.Bind<EnemyMovementBase>().FromInstance(GetComponent<EnemyMovementBase>()).AsSingle();
            Container.Bind<EnemyBase>().FromInstance(GetComponent<EnemyBase>()).AsSingle();

            Container.Bind<EnemyGunBase>().FromComponentInHierarchy().AsSingle();
        }
    }
}
