using Zenject;

namespace Game.Player.Control
{
    public class PlayerFallowerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<PlayerFallower>().FromComponentOn(gameObject).AsSingle();
        }
    }
}