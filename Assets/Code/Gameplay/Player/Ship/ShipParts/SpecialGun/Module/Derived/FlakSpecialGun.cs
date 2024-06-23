using Game.Combat;
using Game.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player.Ship
{
    public class FlakSpecialGun : SpecialGunModuleBase
    {
        public override bool IsGunReadyToShoot => Time.time - _lastShotTime >= _cooldown;

        protected override bool OnTryShoot()
        {
            if (!IsGunReadyToShoot)
                return false;

            _lastShotTime = Time.time;

            GameObject damageDealer = _body.gameObject;
            Transform parent = _playerManager.transform;
            ShootableObjectBase bullet = _shootableObjectPrototype.CreateCopy(damageDealer, parent);
            
            if(bullet is FlakBullet flakBullet)
            {
                Vector2 mousePos = Input.CursorPosition.ReadValue<Vector2>();
                Vector2 targetPos = Utils.ScreanPositionOn2DIntersection(mousePos);

                flakBullet.SetTarget(targetPos);
            }

            bullet.Shoot(_body, transform);

            return true;
        }
    }
}
