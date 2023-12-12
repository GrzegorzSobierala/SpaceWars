using Game.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class TestEnemy : EnemyBase
    {
        public override void GetDamage(Collision2D collsion, DamageData damage)
        {
            ChangeCurrentHp(-damage.BaseDamage);
        }
    }
}
