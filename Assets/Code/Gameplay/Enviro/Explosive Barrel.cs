using Game.Combat;
using UnityEngine;

namespace Game.Room.Enviro
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ExplosiveBarrel : MonoBehaviour , IHittable
    {
        [SerializeField] private DamageAreaExplosion areaExplosionPrefab;
        [Space]
        [SerializeField] private float _collisionSpeedToExplode = 20;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            ChceckCollision(collision);
        }

        public void GetHit(DamageData damage)
        {
            Explode();
        }

        private void ChceckCollision(Collision2D collision)
        {
            float relativeSpeed = collision.relativeVelocity.magnitude;
            if (relativeSpeed > _collisionSpeedToExplode)
            {
                Explode();
            }
        }

        private void Explode()
        {
            areaExplosionPrefab.CreateCopy(gameObject, transform).Explode();
            Destroy(gameObject);
        }
    }
}
