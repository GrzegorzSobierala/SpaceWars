using Game.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class EnemyDamageHandler : MonoBehaviour , IHittable
    {
        ParticleSystem _particleSystem;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        public void OnHit()
        {
            _particleSystem.Play();
        }
    }
}
