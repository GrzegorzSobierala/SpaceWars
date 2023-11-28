using Game.Combat;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class EnemyDamageHandler : DamageHandlerBase
    {
        protected override DamageData ModifyDamage(Collision2D collsion, DamageData damage)
        {
            return damage;
        }
    }
}
