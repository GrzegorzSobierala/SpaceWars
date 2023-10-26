using Zenject;

namespace Game.Player.Ship
{
    public class PlayerViewfinderInstaler : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ViewfinderModuleBase>().FromInstance(GetComponent<ViewfinderModuleBase>()).AsSingle();
        }
    }
}
