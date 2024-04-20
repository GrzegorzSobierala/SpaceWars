using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public struct DamageData
    {
        public GameObject DamageDealer { get; private set; }
        public float BaseDamage {get ; private set;}
        public Vector2 HitPoint { get; private set; }

        public DamageData(GameObject damageDealer, float baseDamge, Vector2 hitPoint)
        {
            DamageDealer = damageDealer;
            BaseDamage = baseDamge;
            HitPoint = hitPoint;
        }
    }
}
