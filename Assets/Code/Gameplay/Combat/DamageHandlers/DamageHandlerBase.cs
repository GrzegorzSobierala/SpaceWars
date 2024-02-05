using System;
using UnityEngine;

namespace Game.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class DamageHandlerBase : MonoBehaviour, IHittable
    {
        public Collider2D Collider => _collider;

        private Action<Collision2D, DamageData> OnGetHit;

        private Collider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }

        public void GetHit(Collision2D collsion, DamageData damage)
        {
            damage = ModifyDamage(collsion, damage);
            OnGetHit?.Invoke(collsion, damage);
        }

        protected abstract DamageData ModifyDamage(Collision2D collsion, DamageData damage);

        public void Subscribe(Action<Collision2D, DamageData> onGetHit)
        {
            OnGetHit += onGetHit;
        }

        public void Unsubscribe(Action<Collision2D, DamageData> onGetHit)
        {
            OnGetHit -= onGetHit;
        }
    }
}
