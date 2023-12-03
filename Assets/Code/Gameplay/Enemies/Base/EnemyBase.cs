using Game.Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public abstract class EnemyBase : MonoBehaviour
    {
        //[Inject] EnemyMovementBase _enemyMovement;
        //[Inject] EnemyGunBase _enemyGun;
        //[Inject] EnemyStateMachine _stateMachine;
        [Inject] List<DamageHandlerBase> _damageHandlers;

        [SerializeField] private float _baseHp = 5f;

        protected float _maxHp;
        protected float _currentHp;
        protected EnemyState _state = EnemyState.Combat;
        protected bool _instantDestroyOnDefeat = false;

        public abstract void GetDamage(Collision2D collsion, DamageData damage);

        protected abstract void Defeated();

        protected virtual void Awake()
        {
            SetStartHP();

            foreach (TestEnemyDamageHandler handler in _damageHandlers)
            {
                handler.Subscribe(GetDamage);
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (TestEnemyDamageHandler handler in _damageHandlers)
            {
                if (handler != null)
                    return;

                handler.Unsubscribe(GetDamage);
            }
        }

        protected void ChangeCurrentHp(float hpChange)
        {
            float newCurrentHp = _currentHp + hpChange;
            
            _currentHp = Mathf.Clamp(newCurrentHp, 0, _maxHp);

            if(_currentHp == 0)
            {
                GetDefeated();
                return;
            }
        }

        private void SetStartHP()
        {
            _maxHp = _baseHp;
            _currentHp = _baseHp;
        }

        private void GetDefeated()
        {
            Defeated();
            _state = EnemyState.Defeat;
            if(_instantDestroyOnDefeat)
            {
                Destroy(gameObject);
            }
        }
    }
}
