using System.Collections;
using UnityEngine;

namespace Game.Player.Ship
{
    public class Rocket : ShootableObjectBase
    {
        [Header("Rocket properties")]
        [SerializeField] private float _shootSpeedMulti = 0.1f;
        [SerializeField] private float _timeToLunch = 0.5f;

        public override void Shoot(Rigidbody2D creatorBody, Transform gunTransform)
        {
            gameObject.SetActive(true);

            transform.position = gunTransform.position;
            _body.position = gunTransform.position;
            _body.rotation = gunTransform.rotation.eulerAngles.z;

            SlowVelocityX(gunTransform, creatorBody.velocity, _horizontalMoveInpactMulti);

            float targetForce = _speed * _body.mass;
            _body.AddRelativeForce(Vector2.up * targetForce * _shootSpeedMulti, ForceMode2D.Impulse);

            _shootTime = Time.time;
            _shootPos = _body.position;
            _shootShipSpeed = GetForwardSpeed(gunTransform, creatorBody.velocity);

            StartCoroutine(WaitAndLaunch());
            StartCoroutine(DestroyByDistance());
        }


        public override void OnHit()
        {
            PlayrParticlesAndDie();
        }

        private IEnumerator WaitAndLaunch()
        {
            yield return new WaitForSeconds(_timeToLunch);

            SlowVelocityX(transform, _body.velocity, 0);

            _body.AddRelativeForce(Vector2.up * _speed, ForceMode2D.Impulse);
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
