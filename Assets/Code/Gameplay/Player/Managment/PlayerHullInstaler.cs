using Zenject;

namespace Game.Player.Ship
{
    public class PlayerHullInstaler : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<PlayerHullModuleBase>().FromInstance(GetComponent<PlayerHullModuleBase>()).AsSingle();
        }
    }
}
