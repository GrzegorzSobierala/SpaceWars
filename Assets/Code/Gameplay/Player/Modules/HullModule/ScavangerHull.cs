using Game.Player.Upgrade;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player.Modules
{
    public class ScavangerHull : PlayerHullBase
    {
        public override void OnHit()
        {
            Debug.Log("ScavangerHull.OnHit()");
        }

       
    }
}
