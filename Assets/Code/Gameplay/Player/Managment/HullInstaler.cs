using Zenject;
using Game.Combat;

namespace Game.Player.Ship
{
    public class HullInstaler : MonoInstaller<HullInstaler>
    {
        public override void InstallBindings()
        {
            Container.Bind<HullModuleBase>().FromInstance(GetComponent<HullModuleBase>()).AsSingle();
            Container.Bind<DamageHandlerBase>().FromComponentsInHierarchy().AsSingle();
        }
    }
}
