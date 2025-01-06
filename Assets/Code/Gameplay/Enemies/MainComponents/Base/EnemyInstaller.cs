using Game.Combat;
using UnityEngine;
using Zenject;
using Game.Utility;

namespace Game.Room.Enemy
{
    public class EnemyInstaller : MonoInstaller<EnemyInstaller>
    {
        [SerializeField] private bool enable0CountEnemyDamageHandlers = true;
        [SerializeField] private bool enable0CountFOVs = true;

        public override void InstallBindings()
        {
            Utils.BindComponentsInChildrens<EnemyDamageHandler>(Container, gameObject,
                true,enable0CountEnemyDamageHandlers);
            Utils.BindComponentsInChildrens<EnemyFieldOfView>(Container, gameObject,
                true, enable0CountFOVs);

            Container.Bind<EnemyCombatStateBase>().FromComponentInChildren().AsSingle();
            Container.Bind<EnemyDefeatedStateBase>().FromComponentInChildren().AsSingle();
            Container.Bind<EnemyGuardStateBase>().FromComponentInChildren().AsSingle();
            Container.Bind<EnemyStateMachineBase>().FromComponentInChildren().AsSingle();

            Utils.BindGetComponent<Rigidbody2D>(Container, gameObject);
            Utils.BindGetComponent<EnemyMovementBase>(Container, gameObject);
            Utils.BindGetComponent<EnemyBase>(Container, gameObject);

            Container.Bind<AlarmActivatorTimer>().FromComponentInHierarchy().AsSingle();
        }
    }
}
