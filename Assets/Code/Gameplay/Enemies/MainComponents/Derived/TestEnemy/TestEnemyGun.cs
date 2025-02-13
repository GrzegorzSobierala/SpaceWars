using Game.Combat;
using Game.Utility;
using Game.Management;
using UnityEngine;
using Zenject;
using UnityEngine.Events;

namespace Game.Room.Enemy
{
    public class TestEnemyGun : EnemyGunBase
    {
        private Vector2 PlayerPos => _playerManager.PlayerBody.position;

        [Inject] private PlayerManager _playerManager;
        [Inject] private EnemyManager _enemyManager;
        
        [SerializeField] private Transform _gunTransform;
        [SerializeField] private TestEnemyBullet _enemyBulletPrototype;
        [SerializeField] private float _shotInterval = 2f;

        [SerializeField] private UnityEvent OnBeforeShoot;

        private float _lastShotTime = 0f;

        public override void Prepare()
        {
            _lastShotTime = Time.time;
        }

        protected override void OnAimingAt(Transform target)
        {
            Vector2 gunPos = (Vector2)_gunTransform.position;
            float angleDegrees;

            angleDegrees = Utils.AngleDirected(gunPos, target.position);
            
            Quaternion rotation = Quaternion.Euler(0, 0, angleDegrees);
            _gunTransform.rotation = rotation;

            OnAimTarget?.Invoke();
        }

        protected override void OnShooting()
        {
            if (Vector2.Distance(transform.position, PlayerPos) > _enemyBulletPrototype.MaxDistance)
                return;

            float nextShootTime = _lastShotTime + _shotInterval;

            if (Time.time < nextShootTime)
            {
                return;
            }

            if(TryShoot())
            {
                _lastShotTime = Time.time;
            }
        }

        protected override void OnShoot()
        {
            Shoot();
        }

        private void Shoot()
        {
            GameObject damageDealer = _body.gameObject;
            Transform parent = _enemyManager.transform;
            _enemyBulletPrototype.CreateCopy(damageDealer, parent).Shoot(null, _gunTransform);
        }
    }
}
