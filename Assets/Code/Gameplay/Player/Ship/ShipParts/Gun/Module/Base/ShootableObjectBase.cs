using UnityEngine;
using Game.Combat;

namespace Game.Player.Ship
{
    public abstract class ShootableObjectBase : MonoBehaviour, IShootable
    {
        [SerializeField] protected Rigidbody2D _body;
        [SerializeField] protected ParticleSystem _particleSystem;

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

    }
}
