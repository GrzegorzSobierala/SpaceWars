using UnityEngine;

namespace Game.Player.Ship
{
    public class FatSpecialGun : SpecialGunModuleBase
    {
        public override bool IsGunReadyToShoot => Time.time - _lastShotTime >= _cooldown;

        protected override bool OnTryShoot()
        {
            if (!IsGunReadyToShoot)
                return false;

            _lastShotTime = Time.time;

            GameObject damageDealer = _body.gameObject;
            Transform parent = _playerManager.transform;
            _shootableObjectPrototype.CreateCopy(damageDealer, parent).Shoot(_body, transform);
            return true;
        }
    }
}
