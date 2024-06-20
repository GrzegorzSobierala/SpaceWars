using Game.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public class FatBullet : ShootableObjectBase
    {
        [SerializeField] private DamageAreaExplosion _areaExplosionPrefab;
        [SerializeField] private Collider2D _proximityTrigger;
        [Space]
        [SerializeField] private LayerMask _triggerActivateLayerMask;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            CheckTrigger(collision);
        }

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

            StartCoroutine(WaitAndDestroy());
        }

        public override void OnHit()
        {
            Explode();
        }

        private void Explode()
        {
            _areaExplosionPrefab.CreateCopy(DamageDealer, transform).Explode();

            _particleSystem.transform.SetParent(null);
            _particleSystem.Play();

            Destroy(gameObject);
        }

        private IEnumerator WaitAndDestroy()
        {
            yield return new WaitUntil(() => SchouldNukeMySelf);

            _areaExplosionPrefab.CreateCopy(DamageDealer, transform).Explode();

            _particleSystem.transform.SetParent(null);
            _particleSystem.Play();

            Destroy(gameObject);
        }

        private bool CheckTrigger(Collider2D collider)
        {
            if (!Utils.ContainsLayer(_triggerActivateLayerMask, collider.gameObject.layer))
                return false;

            Explode();
            return true;
        }
    }
}
