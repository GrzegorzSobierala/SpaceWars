using System;
using UnityEngine;

namespace Game.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class DamageHandlerBase : MonoBehaviour, IHittable
    {
        public Collider2D Collider => _collider;

        private Action<DamageData> OnGetHit;

        private Collider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }

        public void GetHit(DamageData damage)
        {
            damage = ModifyDamage(damage);
            OnGetHit?.Invoke(damage);
        }

        protected abstract DamageData ModifyDamage(DamageData damage);

        public void Subscribe(Action<DamageData> onGetHit)
        {
            OnGetHit += onGetHit;
        }

        public void Unsubscribe(Action<DamageData> onGetHit)
        {
            OnGetHit -= onGetHit;
        }
    }
}
