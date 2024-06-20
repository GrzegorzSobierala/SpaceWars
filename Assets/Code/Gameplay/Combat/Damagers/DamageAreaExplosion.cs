using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Combat
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class DamageAreaExplosion : MonoBehaviour
    {
        [SerializeField] private float _aliveTime = 1.5f;
        [SerializeField] private float _damage = 1;
        [SerializeField] private LayerMask _layerMask;
        [Space]
        [SerializeField] private UnityEvent OnDamageHitEvent;
        [SerializeField] private UnityEvent OnEndExplosion;

        private CircleCollider2D _collider;
        private GameObject _damageDealer;

        private void Awake()
        {
            Init();
        }

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

            ContactFilter2D contactFilter = new ContactFilter2D
            {
                useTriggers = false,
                layerMask = _layerMask,
                useLayerMask = true
            };

            List<Collider2D> overlapResults = new();
            _collider.OverlapCollider(contactFilter, overlapResults);

            foreach (Collider2D collider in overlapResults)
            {
                HitCollider(collider);
            }
        }

        private void Init()
        {
            _collider = GetComponent<CircleCollider2D>();
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
