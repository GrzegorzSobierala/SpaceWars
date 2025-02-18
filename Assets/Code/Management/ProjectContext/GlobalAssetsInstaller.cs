using Game.Utility;
using Zenject;

namespace Game.Management
{
    public class GlobalAssetsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Utils.BindGetComponent<GlobalAssets>(Container, gameObject, true);
        }
    }
}
