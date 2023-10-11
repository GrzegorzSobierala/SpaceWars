using System.Collections;
using UnityEngine;

namespace Game.Player.Modules
{
    public class PlayerRocket : ShootableObjectBase
    {
        [SerializeField] float _startSpeed = 30f;
       
        public override void Shoot(Rigidbody2D creatorBody)
        {
            _body.position = creatorBody.position;
            _body.rotation = creatorBody.rotation;
            _body.velocity = creatorBody.velocity;

            _body.AddForce(creatorBody.transform.up * _startSpeed, ForceMode2D.Impulse);

            StartCoroutine(DestroyByTime());
        }

        public override void OnHit()
        {
            PlayrParticlesAndDie();
        }

        private IEnumerator DestroyByTime()
        {
            yield return new WaitForSeconds(5.0f);

            PlayrParticlesAndDie();
        }

        private void PlayrParticlesAndDie()
        {
            _particleSystem.transform.SetParent(null);
            _particleSystem.Play();
            Destroy(gameObject);
        }
    }
}
