using Game.Combat;
using UnityEngine;
using Zenject;
using Game.Utility;

namespace Game.Room.Enemy
{
    public class EnemyInstaller : MonoInstaller<EnemyInstaller>
    {
        public override void InstallBindings()
        {
            Utils.BindComponentsInChildrens<EnemyDamageHandler>(Container);
            Utils.BindComponentsInChildrens<EnemyFieldOfView>(Container);

            Container.Bind<EnemyCombatStateBase>().FromComponentInChildren().AsSingle();
            Container.Bind<EnemyDefeatedStateBase>().FromComponentInChildren().AsSingle();
            Container.Bind<EnemyGuardStateBase>().FromComponentInChildren().AsSingle();
            Container.Bind<EnemyStateMachineBase>().FromComponentInChildren().AsSingle();

            Utils.BindGetComponent<Rigidbody2D>(Container);
            Utils.BindGetComponent<EnemyMovementBase>(Container);
            Utils.BindGetComponent<EnemyBase>(Container);

            Container.Bind<AlarmActivatorTimer>().FromComponentInHierarchy().AsSingle();
        }
    }
}
