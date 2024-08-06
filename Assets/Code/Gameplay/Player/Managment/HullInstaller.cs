using Zenject;
using Game.Combat;
using Game.Utility;

namespace Game.Player.Ship
{
    public class HullInstaller : MonoInstaller<HullInstaller>
    {
        public override void InstallBindings()
        {
            Utils.BindGetComponent<HullModuleBase>(Container);
            Utils.BindComponentsInChildrens<DamageHandlerBase>(Container);
        }
    }
}
