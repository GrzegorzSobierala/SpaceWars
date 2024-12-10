using Game.Combat;

namespace Game.Room.Enviro
{
    public class DestroyableThingDamageHandler : DamageHandlerBase
    {
        protected override DamageData ModifyDamage(DamageData damage)
        {
            return damage;
        }
    }
}
