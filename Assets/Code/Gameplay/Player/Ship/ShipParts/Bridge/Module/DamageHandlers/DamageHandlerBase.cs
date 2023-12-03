using System;
using UnityEngine;

namespace Game.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class DamageHandlerBase : MonoBehaviour, IHittable
    {
        private Action<Collision2D, DamageData> OnGetHit;

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
