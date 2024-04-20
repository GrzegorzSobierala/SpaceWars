using Game.Combat;
using Game.Player.VirtualCamera;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public class ScavangerHull : HullModuleBase
    {
        

        public override void OnGetHit(DamageData damage)
        {
            ChangeCurrentHp(-damage.BaseDamage);
        }

        protected override void Defeated()
        {
        }
    }
}
