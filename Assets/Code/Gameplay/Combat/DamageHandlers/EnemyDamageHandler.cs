using Game.Combat;

namespace Game.Room.Enemy
{
    public class EnemyDamageHandler : DamageHandlerBase 
    {
        protected override DamageData ModifyDamage(DamageData damage)
        {
            return damage;
        }
    }
}
