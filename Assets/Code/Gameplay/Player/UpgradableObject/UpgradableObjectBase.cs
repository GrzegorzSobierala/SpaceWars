using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Upgrade;

namespace Game.Player
{
    public abstract class UpgradableObjectBase : MonoBehaviour
    {
        protected UpgradeOrder upgradeOrder;

        protected List<UpgradeBase> upgrades = new List<UpgradeBase>();

        public abstract bool TryAddUpgrade(UpgradeBase upgrade);
    }
}
