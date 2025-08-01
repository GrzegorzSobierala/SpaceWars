using Game.Combat;
using System.Collections;
using UnityEngine;

namespace Game.Combat
{
    public class Laser : ShootableObjectBase
    {
        public override void Shoot(Rigidbody2D creatorBody, Transform gunTransform)
        {
            gameObject.SetActive(true);

            _body.position = gunTransform.position;
            _body.rotation = gunTransform.rotation.eulerAngles.z;

            SlowVelocityX(gunTransform, creatorBody.velocity, _horizontalMoveInpactMulti);

            float targetForce = _speed * _body.mass;
            _body.AddRelativeForce(Vector2.up * targetForce, ForceMode2D.Impulse);

            _shootTime = Time.time;
            _shootPos = _body.position;
            _shootShipSpeed = GetForwardSpeed(gunTransform, creatorBody.velocity);
;
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
