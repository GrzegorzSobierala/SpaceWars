using Zenject;

namespace Game.Player.Ship
{
    public class HullInstaler : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<HullModuleBase>().FromInstance(GetComponent<HullModuleBase>()).AsSingle();
        }
    }
}
