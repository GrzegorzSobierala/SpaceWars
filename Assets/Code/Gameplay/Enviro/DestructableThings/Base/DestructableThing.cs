using Game.Combat;
using Game.Room.Enviro;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using Zenject;

namespace Game
{
    public abstract class DestructableThing : MonoBehaviour
    {
        [Inject] protected List<DestroyableThingDamageHandler> _damageHandlers;

        [ShowNonSerializedField] protected float _maxHp;
        [ShowNonSerializedField] protected float _currentHp;

        [SerializeField] private float _baseHp = 5f;
        [SerializeField] private UnityEvent _onDestructEvent;
        [SerializeField] private UnityEvent _onHpChange;

        private bool _isDestructed = false;

        public float CurrentHp => _currentHp;
        public float MaxHp => _maxHp;
        public float BaseHp => _baseHp;

        protected virtual void Awake()
        {
            SetStartHP();

            foreach (DestroyableThingDamageHandler handler in _damageHandlers)
            {
                handler.Subscribe(GetDamage);
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (DestroyableThingDamageHandler handler in _damageHandlers)
            {
                if (handler != null)
                    return;

                handler.Unsubscribe(GetDamage);
            }
        }

        public abstract void GetDamage(DamageData damage);

        protected abstract void OnDestruct(DamageData lastHit);

        protected void SubtractCurrentHp(DamageData damage)
        {
            if (_isDestructed)
                return;

            if (damage.BaseDamage < 0)
            {
                Debug.LogError($"Can't subtract minus number from current hp");
                return;
            }

            if (damage.BaseDamage == 0)
                return;

            ChangeCurrentHp(-damage.BaseDamage);

            if (_currentHp <= 0)
            {
                Destruct(damage);
            }
        }

        protected void AddCurrentHp(float hp)
        {
            if (_isDestructed)
                return;

            if (hp < 0)
            {
                Debug.LogError($"Can't add minus number to current hp");
                return;
            }

            if (hp == 0)
                return;

            ChangeCurrentHp(hp);
        }

        private void ChangeCurrentHp(float hpChange)
        {
            float newCurrentHp = _currentHp + hpChange;

            _currentHp = Mathf.Clamp(newCurrentHp, 0, _maxHp);

            _onHpChange?.Invoke();
        }

        private void Destruct(DamageData damage)
        {
            _isDestructed = true;
            OnDestruct(damage);
            _onDestructEvent?.Invoke();
        }

        private void SetStartHP()
        {
            _maxHp = _baseHp;
            _currentHp = _baseHp;
        }
    }
}
