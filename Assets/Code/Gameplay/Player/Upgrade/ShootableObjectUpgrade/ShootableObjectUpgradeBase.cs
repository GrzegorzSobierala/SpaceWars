using UnityEngine;
using Game.Combat;
using Game.Player.Upgrade;

namespace Game.Upgrade
{
    public abstract class ShootableObjectUpgradeBase : UpgradeBase , IShootable
    {
        public abstract void Shoot(Rigidbody2D creatorBody);

        public abstract void OnHit();
    }
}
