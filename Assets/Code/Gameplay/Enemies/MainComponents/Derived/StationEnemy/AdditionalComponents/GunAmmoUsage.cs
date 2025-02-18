using UnityEngine;

namespace Game.Room.Enemy
{
    public class GunAmmoUsage : MonoBehaviour
    {
        [SerializeField] private int _ammoPerShoot = 1;

        public int AmmoPerShot => _ammoPerShoot;
    }
}
