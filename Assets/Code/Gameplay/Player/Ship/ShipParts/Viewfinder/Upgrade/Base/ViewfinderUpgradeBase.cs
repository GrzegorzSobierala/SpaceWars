using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player.Ship
{
    public abstract class ViewfinderUpgradeBase : ViewfinderBase, IUpgrade
    {
        public abstract IUpgrade Instatiate();
    }
}
