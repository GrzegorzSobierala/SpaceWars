using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class DamageAreaExplosion : MonoBehaviour
    {
        [SerializeField] private float _aliveTime = 1.5f;
        [SerializeField] private float _damage = 1;
        [Space]
        [SerializeField] private UnityEvent OnDamageHitEvent;
        [SerializeField] private UnityEvent OnEndExplosion;

        private GameObject _damageDealer;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            HitCollider(collider);
        }

        public DamageAreaExplosion CreateCopy(GameObject damageDealer, Transform parent)
        {
            DamageAreaExplosion instance = Instantiate(this, parent);

            instance.transform.SetParent(parent.parent);

            instance.gameObject.SetActive(false);
            instance._damageDealer = damageDealer;

            return instance;
        }

        public void Explode()
        {
            gameObject.SetActive(true);
            StartCoroutine(WaitAndEndExplosion());
        }

        private IEnumerator WaitAndEndExplosion()
        {
            yield return new WaitForSeconds(_aliveTime);

            OnEndExplosion?.Invoke();
            Destroy(gameObject);
        }

        private void HitCollider(Collider2D collider)
        {
            if (collider == null)
            {
                return;
            }

            IHittable[] hittables = collider.GetComponents<IHittable>();
            foreach (IHittable hittable in hittables)
            {
                if (hittable == null)
                    continue;

                Vector2 hitPoint = collider.ClosestPoint(transform.position);

                DamageData damage = new DamageData(_damageDealer, _damage, hitPoint);

                hittable.GetHit(damage);
            }

            if (hittables.Length > 0)
            {
                OnDamageHitEvent?.Invoke();
            }
        }
    }
}
