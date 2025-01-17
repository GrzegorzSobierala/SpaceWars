using Game.Combat;
using System.Collections.Generic;
using Zenject;

namespace Game.Room.Enemy
{
    public class StationEnemy : EnemyBase
    {
        [Inject] private List<SilosHp> _silosList;

        public override void GetDamage(DamageData damage)
        {
            float allSilosHp = GetAllSilosCurrentHP();
            
            ChangeCurrentHpTo(allSilosHp);
        }

        protected override void SetStartHP()
        {
            float allSilosHp = GetAllSilosBaseHP();

            _maxHp = allSilosHp;
            _currentHp = allSilosHp;
        }

        private float GetAllSilosBaseHP()
        {
            float allSilosHp = 0;

            foreach (var silos in _silosList)
            {
                allSilosHp += silos.BaseHp;
            }

            return allSilosHp;
        }

        private float GetAllSilosCurrentHP()
        {
            float allSilosHp = 0;

            foreach (var silos in _silosList)
            {
                allSilosHp += silos.CurrentHp;
            }

            return allSilosHp;
        }
    }
}
