using Game.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
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
