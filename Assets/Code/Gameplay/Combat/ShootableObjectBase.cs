using UnityEngine;
using Game.Combat;
using Zenject;
using Game.Room.Enemy;

namespace Game.Combat
{
    public abstract class ShootableObjectBase : MonoBehaviour, IShootable
    {
        public float MaxDistance => _maxDistance;

        protected bool SchouldNukeMySelf
        {
            get
            {
                float maxDistance = _maxDistance + (_shootShipSpeed * _speedMaxDistanceAliveMulti);
                if (Vector2.Distance(_shootPos, _body.position) > maxDistance)
                    return true;

                if (Time.time > _maxTimeAlive + _shootTime)
                    return true;

                return false;
            }
        }

        [Header("Base Depedencies")]
        [SerializeField] protected Rigidbody2D _body;
        [SerializeField] protected ParticleSystem _particleSystem;

        [Header("Base properties")]
        [SerializeField] protected float _damage = 1f;
        [SerializeField] protected float _speed = 30f;
        [SerializeField] protected float _horizontalMoveInpactMulti = 0.20f;
        [SerializeField] protected float _maxTimeAlive = 5f;
        [SerializeField] protected float _maxDistance = 30f;
        [SerializeField] protected float _speedMaxDistanceAliveMulti = 0.5f;

        protected float _shootTime;
        protected float _shootShipSpeed;
        protected Vector2 _shootPos;

        private GameObject _damageDealer;

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider == null)
            {
                OnHit();
                return;
            }

            IHittable[] hittables = collision.collider.GetComponents<IHittable>();
            foreach (IHittable hittable in hittables)
            {
                if (hittable == null)
                    continue;

                DamageData damage = new DamageData(_damageDealer, _damage);

                hittable.GetHit(collision, damage);
            }

            OnHit();
        }

        public virtual ShootableObjectBase CreateCopy(GameObject damageDealer, Transform parent = null)
        {
            ShootableObjectBase instance = Instantiate(this, parent);

            instance.gameObject.SetActive(false);
            instance._damageDealer = damageDealer;
            instance.transform.position = damageDealer.transform.position;
            instance.transform.rotation = damageDealer.transform.rotation;

            return instance;
        }

        public abstract void Shoot(Rigidbody2D creatorBody, Transform gunTransform);

        public abstract void OnHit();

        protected void SlowVelocityX(Transform relativeTo, Vector2 velocity, float slowMulti)
        {
            Vector2 localVelocity = relativeTo.InverseTransformDirection(velocity);
            localVelocity.x *= slowMulti;
            _body.velocity = relativeTo.TransformDirection(localVelocity);
        }

        protected float GetForwardSpeed(Transform relativeTo, Vector2 velocity)
        {
            Vector2 localVelocity = relativeTo.InverseTransformDirection(velocity);
            localVelocity.x = 0;
            Vector2 projectileForwardVelocity = new Vector2(0, localVelocity.y);
            float speed = Vector2.Distance(Vector2.zero, projectileForwardVelocity);
            return speed * Mathf.Max(0, Mathf.Sign(localVelocity.y));
        }
    }
}
