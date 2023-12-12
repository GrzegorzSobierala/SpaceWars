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
        [Inject] protected EnemyStateMachineBase _stateMachine;
        [Inject] protected List<DamageHandlerBase> _damageHandlers;

        [SerializeField] private float _baseHp = 5f;

        protected float _maxHp;
        protected float _currentHp;

        public abstract void GetDamage(Collision2D collsion, DamageData damage);

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

        protected virtual void ChangeCurrentHp(float hpChange)
        {
            float newCurrentHp = _currentHp + hpChange;
            
            _currentHp = Mathf.Clamp(newCurrentHp, 0, _maxHp);

            if (_currentHp == 0 && _stateMachine.CurrentState is not EnemyDefeatedStateBase)
            {
                _stateMachine.SwitchToDefeatedState();
                return;
            }

            if (_currentHp > 0 && _stateMachine.CurrentState is EnemyDefeatedStateBase)
            {
                _stateMachine.SwitchToCombatState();
                return;
            }
        }

        private void SetStartHP()
        {
            _maxHp = _baseHp;
            _currentHp = _baseHp;
        }
    }
}
