using Game.Utility;
using Zenject;

namespace Game.Management
{
    public class RandomManagerInstaller : MonoInstaller<GameSceneManagerInstaller>
    {
        public override void InstallBindings()
        {
            Utils.BindGetComponent<RandomManager>(Container, gameObject, true);
        }
    }
}
