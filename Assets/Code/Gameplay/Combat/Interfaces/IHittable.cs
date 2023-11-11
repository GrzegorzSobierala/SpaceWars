using UnityEngine;

namespace Game.Combat
{
    public interface IHittable
    {
        public abstract void GetHit(Collision2D collsion, DamageData damage);
    }
}
