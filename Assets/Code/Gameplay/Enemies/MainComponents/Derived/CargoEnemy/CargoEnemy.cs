using Game.Combat;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class CargoEnemy : EnemyBase
    {
        public override void GetDamage(Collision2D collsion, DamageData damage)
        {
            ChangeCurrentHp(-damage.BaseDamage);
        }
    }
}
