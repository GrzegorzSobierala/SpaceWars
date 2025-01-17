namespace Game.Combat
{
    public class ResistDamageHandler : DamageHandlerBase
    {
        protected override DamageData ModifyDamage(DamageData damage)
        {
            return new DamageData(damage.DamageDealer, 0.0f, damage.HitPoint);
        }
    }
}
