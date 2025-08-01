using Game.Objectives;
using Game.Room.Enemy;
using Game.Utility;
using Zenject;

namespace Game.Room
{
    public class RoomSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Utils.BindGetComponent<RoomManager>(Container, gameObject);
            Container.Bind<PlayerObjectsParent>().FromComponentInHierarchy().AsSingle();
            Container.Bind<RoomQuestController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<EnemyManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}