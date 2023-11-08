using UnityEngine;
using Game.Combat;

namespace Game.Player.Ship
{
    public abstract class ShootableObjectBase : MonoBehaviour, IShootable
    {
        [Header("Base Depedencies")]
        [SerializeField] protected Rigidbody2D _body;
        [SerializeField] protected ParticleSystem _particleSystem;

        [Header("Base properties")]
        [SerializeField] protected float _speed = 30f;
        [SerializeField] protected float _horizontalMoveInpactMulti = 0.20f;
        [SerializeField] protected float _maxTimeAlive = 5f;
        [SerializeField] protected float _maxDistance = 30f;
        [SerializeField] protected float _speedMaxDistanceAliveMulti = 0.5f;

        protected float _shootTime;
        protected float _shootShipSpeed;
        protected Vector2 _shootPos;

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

        public virtual ShootableObjectBase CreateCopy()
        {
            ShootableObjectBase instance = Instantiate(this);

            instance.gameObject.SetActive(false);

            return instance;
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.rigidbody == null)
            {
                OnHit();
                return;
            }

            if (collision.rigidbody.TryGetComponent(out IHittable hittable))
            {
                hittable.GetHit(collision);
                OnHit();
                return;
            }

            OnHit();
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
