using Game.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class TargetDummy : EnemyBase
    {
        [SerializeField] private ParticleSystem _hitParticle;
        [SerializeField] private ParticleSystem _defeatParticle;

        protected override void Awake()
        {
            base.Awake();

            if(!_hitParticle.transform.IsChildOf(transform) || 
                !_defeatParticle.transform.IsChildOf(transform))
            {
                Debug.LogError($"_particle must be child of {transform.name}", gameObject);
            }
        }

        public override void GetHit(Collision2D collsion, DamageData damage)
        {
            _hitParticle.Play();
            ChangeCurrentHp(-damage.BaseDamage);
        }

        protected override void Defeated()
        {
            _defeatParticle.transform.SetParent(null);
            _defeatParticle.Play();
            _instantDestroyOnDefeat = true;
        }
    }
}
