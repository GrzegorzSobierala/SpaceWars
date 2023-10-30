using Zenject;

namespace Game.Player.Ship
{
    public class PlayerBridgeInstaler : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<BridgeModuleBase>().FromInstance(GetComponent<BridgeModuleBase>()).AsSingle();
        }
    }
}
