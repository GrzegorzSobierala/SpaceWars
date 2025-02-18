using Game.Combat;
using Game.Room.Shared;
using Zenject;

namespace Game.Room.Enemy
{
    public class SilosHp : DestructableThing
    {
        [Inject] private EnemyBase _station;

        public override void GetDamage(DamageData damage)
        {
            SubtractCurrentHp(damage);

            _station.GetDamage(damage);
        }

        protected override void OnDestruct(DamageData lastHit)
        {
            
        }
    }
}
