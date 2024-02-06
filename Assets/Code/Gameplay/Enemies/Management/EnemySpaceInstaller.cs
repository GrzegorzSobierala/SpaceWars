using Game.Utility;
using Zenject;

namespace Game.Room.Enemy
{
    public class EnemySpaceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Utils.BindComponentsInChildrens<EnemyBase>(Container, false);
            Utils.BindGetComponent<EnemyManager>(Container);
            Utils.BindGetComponent<EnemyRoomAlarm>(Container);
        }
    }
}
