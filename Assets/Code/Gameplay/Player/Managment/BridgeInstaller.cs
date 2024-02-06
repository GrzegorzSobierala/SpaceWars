using Game.Utility;
using Zenject;

namespace Game.Player.Ship
{
    public class BridgeInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Utils.BindGetComponent<BridgeModuleBase>(Container);
        }
    }
}
