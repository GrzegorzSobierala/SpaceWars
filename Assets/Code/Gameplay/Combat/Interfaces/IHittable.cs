using UnityEngine;

namespace Game.Combat
{
    public interface IHittable
    {
        public abstract void GetHit(DamageData damage);
    }
}
