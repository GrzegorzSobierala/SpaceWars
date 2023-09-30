using UnityEngine;
using Game.Upgrade;
using Game.Combat;

namespace Game.Player
{
    public abstract class ShootableObjectBase : UpgradableObjectBase , IShootable
    {
        [SerializeField] protected Rigidbody2D _body;
        [SerializeField] protected ParticleSystem _particleSystem;

        public virtual ShootableObjectBase CreateCopy()
        {
            ShootableObjectBase instance = Instantiate(this);

            return instance;
        }

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.rigidbody == null)
            {
                OnHit();
                return;
            }

            if (collision.rigidbody.TryGetComponent(out IHittable hittable))
            {
                hittable.OnHit();
                OnHit();
                return;
            }

            OnHit();
        }

        public override bool TryAddUpgrade(UpgradeBase upgrade)
        {
            if (upgrade is IShootable)
            {
                upgrades.Add(upgrade);
                return true;
            }
            return false;
        }

        public abstract void Shoot(Rigidbody2D creatorBody);

        public abstract void OnHit();

    }
}
