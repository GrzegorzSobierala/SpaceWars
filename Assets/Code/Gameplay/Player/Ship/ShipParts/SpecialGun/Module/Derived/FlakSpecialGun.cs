using Game.Combat;
using Game.Player.Control;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public class FlakSpecialGun : SpecialGunModuleBase
    {
        [Inject] private CursorCamera _cursorCamera;

        public override bool IsGunReadyToShoot => Time.time - _lastShotTime >= _cooldown;

        protected override bool OnTryShoot()
        {
            if (!IsGunReadyToShoot)
                return false;

            _lastShotTime = Time.time;

            GameObject damageDealer = _body.gameObject;
            ShootableObjectBase bullet = _shootableObjectPrototype.CreateCopy(damageDealer, BulletParent);
            
            if(bullet is FlakBullet flakBullet)
            {
                Vector2 mousePos = Input.CursorPosition.ReadValue<Vector2>();
                Vector2 targetPos = _cursorCamera.ScreanPositionOn2DIntersection(mousePos);

                flakBullet.SetTarget(targetPos);
            }

            bullet.Shoot(_body, transform);

            return true;
        }
    }
}
