using Game.Combat;

namespace Game.Room.Shared
{
    public class BasicDestructableThing : DestructableThing
    {
        public override void GetDamage(DamageData damage)
        {
            SubtractCurrentHp(damage);
        }

        protected override void OnDestruct(DamageData lastHit)
        {

        }
    }
}
