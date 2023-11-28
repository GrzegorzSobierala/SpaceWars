using Game.Combat;
using Zenject;

namespace Game.Room.Enemy
{
    public class EnemyInstaller : MonoInstaller<EnemyInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<DamageHandlerBase>().FromComponentsInHierarchy().AsSingle();
        }
    }
}
