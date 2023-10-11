using UnityEngine;

namespace Game.Player.Upgrade
{
    abstract public class UpgradeBase : MonoBehaviour
    {
        abstract public UpgradeBase CreateInstanceFromPrefab();
    }
}
