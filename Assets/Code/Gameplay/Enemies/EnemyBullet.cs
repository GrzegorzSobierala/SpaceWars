using Game.Combat;
using Game.Player.Ship;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class EnemyBullet : ShootableObjectBase
    {
        public float MaxDistance => _maxDistance;

        public override void Shoot(Rigidbody2D creatorBody, Transform gunTransform)
        {
            gameObject.SetActive(true);

            _body.position = gunTransform.position;
            _body.rotation = gunTransform.rotation.eulerAngles.z;

            float targetForce = _speed * _body.mass;
            _body.AddRelativeForce(Vector2.up * targetForce, ForceMode2D.Impulse);

            _shootTime = Time.time;
            _shootPos = gunTransform.position;
            _shootShipSpeed = 0;

            StartCoroutine(DestroyByDistance());
        }

        public override void OnHit()
        {
            PlayrParticlesAndDie();
        }

        private IEnumerator DestroyByDistance()
        {
            yield return new WaitUntil(() => SchouldNukeMySelf);

            Destroy(gameObject);
        }

        private void PlayrParticlesAndDie()
        {
            _particleSystem.transform.SetParent(null);
            _particleSystem.Play();
            Destroy(gameObject);
        }
    }
}
