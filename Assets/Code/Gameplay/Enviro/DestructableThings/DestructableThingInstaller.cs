using Game.Room.Enviro;
using Game.Utility;
using Zenject;

namespace Game
{
    public class DestructableThingInstaller : MonoInstaller<DestructableThingInstaller>
    {
        public override void InstallBindings()
        {
            Utils.BindComponentsInChildrens<DestroyableThingDamageHandler>(Container, gameObject);
        }
    }
}
