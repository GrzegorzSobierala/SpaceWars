using Game.Utility;
using Zenject;

namespace Game.Management
{
    public class GameSceneManagerInstaller : MonoInstaller<GameSceneManagerInstaller>
    {
        public override void InstallBindings()
        {
            Utils.BindGetComponent<GameSceneManager>(Container, gameObject, true);
        }
    }
}
