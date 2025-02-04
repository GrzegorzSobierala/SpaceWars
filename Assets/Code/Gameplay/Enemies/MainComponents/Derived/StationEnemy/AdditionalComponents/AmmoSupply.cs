using NaughtyAttributes;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class AmmoSupply : MonoBehaviour
    {
        [SerializeField] private int _maxAmmoCapasity = 100;

        private int _currentAmmo;

        public int CurrentAmmo => _currentAmmo;

        private void Awake()
        {
            _currentAmmo = _maxAmmoCapasity;
        }

        public int TakeAmmo(int amount)
        {
            if(_currentAmmo >= amount)
            {
                _currentAmmo -= amount;
                return amount;
            }
            else
            {
                int ammoLeft = _currentAmmo;
                _currentAmmo = 0;
                return ammoLeft;
            }
        }

        public void DestroySupply()
        {
            Destroy(gameObject);
        }
    }
}
