using Game.Player;
using Game.Player.Ship;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player.Ship
{
    public abstract class GunBase : ShipPart, IGun
    {
        public abstract void Shoot();
    }
}
