using Game.Utility;
using Zenject;

namespace Game.Management
{
    public class GameManagerInstaller : MonoInstaller<GameManagerInstaller>
    {
        public override void InstallBindings()
        {
            Utils.BindGetComponent<GameManager>(Container, gameObject, false);
        }

    }
}
