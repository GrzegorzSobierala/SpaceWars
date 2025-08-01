using Game.Utility.Globals;
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
        [SerializeField] private float _explodeForce = 20000000;
        [Space]
        [SerializeField] private UnityEvent OnDamageHitEvent;
        [SerializeField] private UnityEvent OnEndExplosion;

        private GameObject _damageDealer;
        private float _explodeTime;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            HitCollider(collider);
        }

        public DamageAreaExplosion CreateCopy(GameObject damageDealer, Transform creator)
        {
            DamageAreaExplosion instance = Instantiate(this, creator);

            instance.transform.SetParent(creator.parent);

            instance.gameObject.SetActive(false);
            instance._damageDealer = damageDealer;

            return instance;
        }

        public void Explode()
        {
            gameObject.SetActive(true);
            _explodeTime = Time.time;
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

            Vector2 hitPoint = collider.ClosestPoint(transform.position);

            IHittable[] hittables = collider.GetComponents<IHittable>();
            foreach (IHittable hittable in hittables)
            {
                if (hittable == null)
                    continue;

                DamageData damage = new DamageData(_damageDealer, _damage, hitPoint);

                hittable.GetHit(damage);
            }

            if (hittables.Length > 0)
            {
                OnDamageHitEvent?.Invoke();
            }

            ExplodeForce(collider, hitPoint);
        }

        private void ExplodeForce(Collider2D collider, Vector2 hitPoint)
        {
            if (!collider.attachedRigidbody)
                return;

            Rigidbody2D hitBody = collider.attachedRigidbody;

            if(collider.attachedRigidbody.isKinematic)
            {
                if (!hitBody.TryGetComponent(out AgentForceReceiver receiver))
                    return;

                receiver.AddForce(GetExplosionForce(hitBody));
            }
            else
            {
                hitBody.AddForceAtPosition(GetExplosionForce(hitBody), hitPoint);
            }
        }

        private Vector2 GetExplosionForce(Rigidbody2D body)
        {
            if (body.gameObject.layer == LayerMask.NameToLayer(Layers.EnemyCombat)
                || body.gameObject.layer == LayerMask.NameToLayer(Layers.PlayerCombat))
                return Vector2.zero;

            Vector2 force = body.position - (Vector2)transform.position;

            force = force.normalized;
            float aliveTime = Time.time - _explodeTime;
            float aliveForceMulti = (_aliveTime - aliveTime) / _aliveTime;
            force *= Mathf.Clamp(aliveForceMulti, 0.3f, 1);

            return force * _explodeForce;
        }
    }
}
