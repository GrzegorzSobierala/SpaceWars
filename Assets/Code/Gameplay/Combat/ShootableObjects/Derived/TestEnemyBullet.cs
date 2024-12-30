using Game.Combat;
using Game.Utility;
using System.Collections;
using UnityEngine;

namespace Game.Combat
{
    public class TestEnemyBullet : ShootableObjectBase
    {
        public override void Shoot(Rigidbody2D creatorBody, Transform gunTransform)
        {
            gameObject.SetActive(true);

            _body.transform.position = gunTransform.position;
            _body.transform.rotation = Utils.ChangeRotationZ(_body.transform.rotation, 
                gunTransform.rotation.eulerAngles.z);

            _body.position = gunTransform.position;
            _body.rotation = gunTransform.rotation.eulerAngles.z;

            float targetForce = _speed * _body.mass;
            _body.AddRelativeForce(Vector2.up * targetForce, ForceMode2D.Impulse);

            _shootTime = Time.time;
            _shootPos = gunTransform.position;
            _shootShipSpeed = 0;

            StartCoroutine(WaitAndDestroy());
        }

        public override void OnHit()
        {
            PlayrParticlesAndDie();
        }

        private IEnumerator WaitAndDestroy()
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
