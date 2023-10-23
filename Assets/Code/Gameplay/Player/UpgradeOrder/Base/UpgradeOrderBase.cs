using System.Collections.Generic;
using UnityEngine;
 
namespace Game.Player.Ship
{
    public abstract class UpgradeOrderBase<T> : ScriptableObject where T : ShipPart, IUpgrade
    {
        [SerializeField] private List<T> upgrades;

        public List<T> Upgrades => upgrades;
    }
}
