using UnityEngine;

namespace Game.Player.Ship
{
    public class LaserGun : GunModuleBase
    {
        private void Update()
        {
            if (Input.Shoot.ReadValue<float>() == 1.0f)
            {
                TryShoot();
            }
        }

        private void TryShoot()
        {
            if (Time.time - _lastShotTime < _cooldown)
            {
                return;
            }

            Shoot();
        }

        public override void Shoot()
        {
            _lastShotTime = Time.time;

            _shootableObjectPrefab.CreateCopy(_playerManager.transform).Shoot(_body, transform);
        }
    }
}
