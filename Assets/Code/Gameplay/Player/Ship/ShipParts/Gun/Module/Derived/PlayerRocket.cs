using System.Collections;
using UnityEngine;

namespace Game.Player.Ship
{
    public class PlayerRocket : ShootableObjectBase
    {
        [SerializeField] float _startSpeed = 30f;
       
        public override void Shoot(Rigidbody2D creatorBody, Transform gunTransform)
        {
            _body.position = gunTransform.position;
            _body.rotation = gunTransform.rotation.eulerAngles.z;
            _body.velocity = creatorBody.velocity;

            _body.AddForce(gunTransform.up * _startSpeed, ForceMode2D.Impulse);

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
