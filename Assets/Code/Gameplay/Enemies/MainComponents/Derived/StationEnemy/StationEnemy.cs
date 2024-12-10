using Game.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class StationEnemy : EnemyBase
    {
        public const float SILOS_DAMAGE_TO_STATION = 1.0f; 

        [Inject] private List<SilosHp> silosList;

        [Tooltip("Base hp automaitc set by amount of HpTowers")]
        public override void GetDamage(DamageData damage)
        {
            SubtractCurrentHp(damage);
            
            if (_stateMachine.CurrentState is not EnemyDefeatedStateBase 
                && _currentHp < SILOS_DAMAGE_TO_STATION/2.0f)
            {
                Debug.LogError("Kurwa float");
                ChangeCurrentHp(-999999.0f);
            }
        }

        protected override void SetStartHP()
        {
            _maxHp = silosList.Count;
            _currentHp = silosList.Count;
        }
    }
}
