using Zenject;
using Game.Player.Ui;

namespace Game.Player
{
    public class PlayerUiInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<AlarmUi>().FromComponentInHierarchy(false).AsSingle();
            Container.Bind<PlayerUiController>().FromComponentInHierarchy(false).AsSingle();
        }
    }
}
