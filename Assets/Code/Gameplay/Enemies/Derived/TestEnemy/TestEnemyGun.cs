using Game.Utility;
using Game.Management;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class TestEnemyGun : EnemyGunBase
    {
        private Vector2 PlayerPos => _playerManager.PlayerBody.position;

        [Inject] private PlayerManager _playerManager;
        [Inject] private EnemyManager _enemyManager;

        [SerializeField] private Transform _gunTransform;
        [SerializeField] private TestEnemyBullet _enemyBulletPrototype;
        [SerializeField] private float _cooldown = 2f;

        private float _lastShotTime = 0f;

        protected override void OnAimingAt(Transform target)
        {
            Vector2 gunPos = (Vector2)_gunTransform.position;
            float angleDegrees;

            angleDegrees = Utils.AngleDirected(gunPos, target.position) - 90f;
            
            Quaternion rotation = Quaternion.Euler(0, 0, angleDegrees);
            _gunTransform.rotation = rotation;

            OnAimTarget?.Invoke();
        }

        protected override void OnShooting()
        {
            if (Vector2.Distance(transform.position, PlayerPos) > _enemyBulletPrototype.MaxDistance)
                return;

            if (Time.time - _lastShotTime < _cooldown)
            {
                return;
            }

            Shoot();
        }

        private void Shoot()
        {
            _lastShotTime = Time.time;

            _enemyBulletPrototype.CreateCopy(_enemyManager.transform).Shoot(null, _gunTransform);
        }
    }
}
