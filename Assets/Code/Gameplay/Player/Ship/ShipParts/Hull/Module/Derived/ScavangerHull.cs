using Game.Combat;
using UnityEngine;

namespace Game.Player.Ship
{
    public class ScavangerHull : HullModuleBase
    {
        public override void GetHit(Collision2D collsion, DamageData damage)
        {
            ChangeCurrentHp(-damage.BaseDamage);
        }

        protected override void Defeated()
        {
            Debug.Log("ded :(");
        }
    }
}
