using Zenject;
using Game.Combat;

namespace Game.Player.Ship
{
    public class HullInstaller : MonoInstaller<HullInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<HullModuleBase>().FromInstance(GetComponent<HullModuleBase>()).AsSingle();
            Container.Bind<DamageHandlerBase>().FromComponentsInHierarchy().AsSingle();
        }
    }
}
