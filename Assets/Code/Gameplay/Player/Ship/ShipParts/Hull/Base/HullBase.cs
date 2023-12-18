using Game.Combat;
using UnityEngine;

namespace Game.Player.Ship
{
    public abstract class HullBase : ShipPart, IHittable
    {
        public abstract void GetHit(Collision2D collsion, DamageData damage);
    }
}
