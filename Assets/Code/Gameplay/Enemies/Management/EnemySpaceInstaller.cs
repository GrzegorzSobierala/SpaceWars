using Game.Utility;
using Zenject;

namespace Game.Room.Enemy
{
    public class EnemySpaceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Utils.BindComponentsInChildrens<EnemyBase>(Container, gameObject, false);
            Utils.BindGetComponent<EnemyManager>(Container, gameObject);
            Utils.BindGetComponent<EnemyRoomAlarm>(Container, gameObject);
        }
    }
}
