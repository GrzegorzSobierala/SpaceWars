using Game.Utility;
using Zenject;

namespace Game.Management
{
    public class ProjectContextInstaller : MonoInstaller<ProjectContextInstaller>
    {
        public override void InstallBindings()
        {
            Utils.BindGetComponent<CursorCamera>(Container, gameObject, true);
        }
    }
}
