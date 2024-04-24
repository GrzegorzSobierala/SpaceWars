using Game.Combat;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class TestEnemy : EnemyBase
    {
        public override void GetDamage(DamageData damage)
        {
            ChangeCurrentHp(-damage.BaseDamage);
        }
    }
}
