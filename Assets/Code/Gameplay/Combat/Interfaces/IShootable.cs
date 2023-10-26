
using UnityEngine;

namespace Game.Combat
{
    public interface IShootable
    {
        public void Shoot(Rigidbody2D creatorBody, Transform gunTransform);

        public void OnHit();
    }
}
