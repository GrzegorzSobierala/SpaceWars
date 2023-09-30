using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Upgrade
{
    public class UpgradeOrder : ScriptableObject
    {
        [SerializeField] private List<UpgradeBase> upgrades = new List<UpgradeBase>();

        public List<UpgradeBase> Upgrades => upgrades;
    }
}
