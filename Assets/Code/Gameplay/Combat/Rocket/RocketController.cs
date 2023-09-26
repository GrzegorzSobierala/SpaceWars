using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Combat
{
    public class RocketController : MonoBehaviour
    {
        ParticleSystem _particleSystem;
        Rigidbody2D _body;

        public static void InstantiateAndShoot(RocketController rocketPrefab, 
            Rigidbody2D bodyCreator, float force)
        {
            RocketController newRocket;
            newRocket = Instantiate(rocketPrefab);

            newRocket._body.position = bodyCreator.position;    
            newRocket._body.rotation = bodyCreator.rotation;
            newRocket._body.velocity = bodyCreator.velocity;
            newRocket._body.AddForce(bodyCreator.transform.up * force, ForceMode2D.Impulse);
        }

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _body = GetComponent<Rigidbody2D>();
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if(collision.rigidbody == null)
            {
                Boom();
                return;
            }

            if(collision.rigidbody.TryGetComponent(out IHittable hittable))
            {
                hittable.OnHit();
                Boom();
                return;
            }

            Boom();
        }

        private void Boom()
        {
            _particleSystem.Play();
            StartCoroutine(WaitAndDestroy());
        }

        private IEnumerator WaitAndDestroy()
        {
            List<Collider2D> colliders = new List<Collider2D>();
            _body.GetAttachedColliders(colliders);

            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.enabled = false;
            }   

            _body.Sleep();

            yield return new WaitForSeconds(1f);

            Destroy(gameObject);
        }
    }
}
