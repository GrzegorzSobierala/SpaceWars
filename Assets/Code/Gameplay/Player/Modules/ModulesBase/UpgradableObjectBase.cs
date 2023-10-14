using System.Collections.Generic;
using UnityEngine;
using Game.Player.Upgrade;

namespace Game.Player.Modules
{
    public abstract class UpgradableObjectBase : MonoBehaviour
    {
        protected UpgradeOrder upgradeOrder;

        protected List<UpgradeBase> upgrades = new List<UpgradeBase>();

        public abstract bool TryAddUpgrade(UpgradeBase upgrade);
    }
}
