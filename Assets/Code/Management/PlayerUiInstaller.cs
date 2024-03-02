using Zenject;
using Game.Player.UI;

namespace Game.Player
{
    public class PlayerUiInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<TestAlarmUI>().FromComponentInHierarchy(false).AsSingle();
        }
    }
}
