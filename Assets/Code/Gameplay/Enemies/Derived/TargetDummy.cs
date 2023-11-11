using Game.Combat;
using Game.Management;
using Game.Player;
using Game.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using static Zenject.CheatSheet;

namespace Game.Room.Enemy
{
    public class TargetDummy : EnemyBase
    {
        [Inject] private PlayerManager _playerManager;

        [SerializeField] private Transform _gunTransform;
        [SerializeField] private EnemyBullet _enemyBullet;
        [SerializeField] private float _cooldown = 2f;
        [SerializeField] private ParticleSystem _hitParticle;
        [SerializeField] private ParticleSystem _defeatParticle;

        private float _lastShotTime = 0f;

        private Vector2 PlayerPos => _playerManager.PlayerBody.position;

        protected override void Awake()
        {
            base.Awake();

            if(!_hitParticle.transform.IsChildOf(transform) || 
                !_defeatParticle.transform.IsChildOf(transform))
            {
                Debug.LogError($"_particle must be child of {transform.name}", gameObject);
            }
        }

        private void Update()
        {
            AimGun();

            if (Vector2.Distance(transform.position, PlayerPos) <= _enemyBullet.MaxDistance)
            {
                TryShoot();
            }
        }

        private void AimGun()
        {

            Vector2 gunPos = (Vector2)_gunTransform.position;
            float angleDegrees = Utils.AngleDirected(gunPos, PlayerPos) - 90f;

            Quaternion rotation = Quaternion.Euler(0, 0, angleDegrees);
            _gunTransform.rotation = rotation;
        }

        private void TryShoot()
        {
            if (Time.time - _lastShotTime < _cooldown)
            {
                return;
            }

            Shoot();
        }

        private void Shoot()
        {
            _lastShotTime = Time.time;

            _enemyBullet.CreateCopy().Shoot(null, _gunTransform);
        }

        public override void GetHit(Collision2D collsion, DamageData damage)
        {
            _hitParticle.Play();
            ChangeCurrentHp(-damage.BaseDamage);
        }

        protected override void Defeated()
        {
            _defeatParticle.transform.SetParent(null);
            _defeatParticle.Play();
            _instantDestroyOnDefeat = true;
        }
    }
}
