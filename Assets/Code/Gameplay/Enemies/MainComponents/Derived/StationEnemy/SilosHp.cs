using Game.Combat;
using Game.Room.Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game
{
    public class SilosHp : DestructableThing
    {
        [Inject] private EnemyBase _station;

        public override void GetDamage(DamageData damage)
        {
            SubtractCurrentHp(damage);
        }

        protected override void OnDestruct(DamageData lastHit)
        {
            DamageData damageToStation = new DamageData(lastHit.DamageDealer, 
                StationEnemy.SILOS_DAMAGE_TO_STATION, lastHit.HitPoint);

            _station.GetDamage(damageToStation);
        }
    }
}
