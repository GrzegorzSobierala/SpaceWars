using Game.Combat;
using UnityEngine;
using Zenject;
using Game.Utility;

namespace Game.Room.Enemy
{
    public class EnemyInstaller : MonoInstaller<EnemyInstaller>
    {
        [SerializeField] private bool _bindEnemyDamageHandlers = true;
        [SerializeField] private bool _bindFOVs = true;

        public override void InstallBindings()
        {
            Container.Bind<EnemyCombatStateBase>().FromComponentInChildren().AsSingle();
            Container.Bind<EnemyDefeatedStateBase>().FromComponentInChildren().AsSingle();
            Container.Bind<EnemyGuardStateBase>().FromComponentInChildren().AsSingle();
            Container.Bind<EnemyStateMachineBase>().FromComponentInChildren().AsSingle();
            Container.Bind<AlarmActivatorTimer>().FromComponentInHierarchy().AsSingle();

            Utils.BindGetComponent<Rigidbody2D>(Container, gameObject);
            Utils.BindGetComponent<EnemyMovementBase>(Container, gameObject);
            Utils.BindGetComponent<EnemyBase>(Container, gameObject);

            Utils.BindComponentsInChildrens<DamageHandlerBase>(Container, gameObject, true);
            if (_bindEnemyDamageHandlers)
            {
                Utils.BindComponentsInChildrens<EnemyDamageHandler>(Container, gameObject, true);
            }
            if (_bindFOVs)
            {
                Utils.BindComponentsInChildrens<EnemyFieldOfView>(Container, gameObject, true);
            }
        }
    }
}
