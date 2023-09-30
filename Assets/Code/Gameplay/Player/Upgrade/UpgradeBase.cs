using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Upgrade
{
    abstract public class UpgradeBase : MonoBehaviour
    {
        abstract public UpgradeBase CreateInstanceFromPrefab();
    }
}
