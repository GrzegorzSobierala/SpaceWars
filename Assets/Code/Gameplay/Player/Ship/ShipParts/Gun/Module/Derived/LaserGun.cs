using UnityEngine;

namespace Game.Player.Ship
{
    public class LaserGun : GunModuleBase
    {
        protected override bool OnTryShoot()
        {
            if (Time.time - _lastShotTime < _cooldown)
                return false;

            _lastShotTime = Time.time;

            GameObject damageDealer = _body.gameObject;
            Transform parent = _playerManager.transform;
            _shootableObjectPrototype.CreateCopy(damageDealer, parent).Shoot(_body, transform);
            return true;
        }
    }
}
