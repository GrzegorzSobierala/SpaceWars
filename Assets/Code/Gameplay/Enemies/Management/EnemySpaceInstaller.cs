using Game.Physics;
using Game.Utility;
using Zenject;

namespace Game.Room.Enemy
{
    public class EnemySpaceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Utils.BindComponentsInChildrensHash<EnemyBase>(Container, gameObject, true);
            Utils.BindGetComponent<EnemyManager>(Container, gameObject);
            Utils.BindGetComponent<EnemyRoomAlarm>(Container, gameObject);

            Container.Bind<FieldOfViewSystem>().FromComponentInHierarchy(false).AsSingle().NonLazy();
        }
    }
}
