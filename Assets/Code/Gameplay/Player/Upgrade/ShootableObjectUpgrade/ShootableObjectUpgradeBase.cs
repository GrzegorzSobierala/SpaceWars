using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Combat;

namespace Game.Upgrade
{
    public abstract class ShootableObjectUpgradeBase : UpgradeBase , IShootable
    {
        public abstract void Shoot(Rigidbody2D creatorBody);

        public abstract void OnHit();
    }
}
