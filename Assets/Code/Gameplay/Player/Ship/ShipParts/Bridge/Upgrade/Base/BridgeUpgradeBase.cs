using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player.Ship
{
    public abstract class BridgeUpgradeBase : BridgeBase, IUpgrade
    {
        public abstract IUpgrade Instatiate();
    }
}
