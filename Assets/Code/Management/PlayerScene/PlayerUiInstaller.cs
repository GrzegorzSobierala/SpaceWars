using Zenject;
using Game.Player.Ui;
using Game.Objectives;

namespace Game.Player
{
    public class PlayerUiInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<AlarmUi>().FromComponentInHierarchy(false).AsSingle();
            Container.Bind<PlayerUiController>().FromComponentInHierarchy(false).AsSingle();
            Container.Bind<PlayerQuestsUiController>().FromComponentInHierarchy(false).AsSingle();
            Container.Bind<MissionPoinerUi>().FromComponentInHierarchy(false).AsSingle();
        }
    }
}
