using Game.Combat;
using Game.Management;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public abstract class EnemyBase : MonoBehaviour
    {
        public Action<float> OnHpChange;

        [Inject] protected EnemyStateMachineBase _stateMachine;
        [Inject] protected List<EnemyDamageHandler> _damageHandlers;
        [Inject] private GlobalAssets _globalAssets;

        [ShowNonSerializedField] protected float _maxHp;
        [ShowNonSerializedField] protected float _currentHp;

        [SerializeField] private float _baseHp = 5f;
        [SerializeField] private ArrowParameters _arrowParameters;

        public EnemyStateMachineBase StateMachine => _stateMachine;
        public float CurrentHp => _currentHp;
        public float MaxHp => _maxHp;
        public ArrowParameters ArrowParameters => _arrowParameters;

        protected virtual void Awake()
        {
            SetStartHP();

            foreach (EnemyDamageHandler handler in _damageHandlers)
            {
                handler.Subscribe(GetDamage);
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (EnemyDamageHandler handler in _damageHandlers)
            {
                if (handler != null)
                    return;

                handler.Unsubscribe(GetDamage);
            }
        }

        public abstract void GetDamage(DamageData damage);

        public void GetHeal(float hp)
        {
            AddCurrentHp(hp);
        }

        protected virtual void SetStartHP()
        {
            _maxHp = _baseHp;
            _currentHp = _baseHp;
        }

        protected void SubtractCurrentHp(DamageData damage)
        {
            if (damage.BaseDamage < 0)
            {
                Debug.LogError($"Can't subtract minus number from current hp");
                return;
            }

            if (damage.BaseDamage == 0)
                return;

            ChangeCurrentHpBy(-damage.BaseDamage);
            OnDamageVisualEffect();
        }

        protected void AddCurrentHp(float hp)
        {
            if (hp < 0)
            {
                Debug.LogError($"Can't add minus number to current hp");
                return;
            }

            if (hp == 0)
                return;

            ChangeCurrentHpBy(hp);
        }

        protected void ChangeCurrentHpBy(float hpChange)
        {
            float newCurrentHp = _currentHp + hpChange;

            ChangeCurrentHpTo(newCurrentHp);
        }

        protected void ChangeCurrentHpTo(float newHp)
        {
            float newHpClamped = Mathf.Clamp(newHp, 0, _maxHp);

            if (_currentHp == newHpClamped)
                return;

            _currentHp = newHpClamped;

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

            OnHpChange?.Invoke(_currentHp);
        }

        bool _onDamageVisualEffectInProgress = false;

        private void OnDamageVisualEffect()
        {
            if(_onDamageVisualEffectInProgress)
            {
                CancelInvoke(nameof(RestoreMaterial));
                Invoke(nameof(RestoreMaterial), _globalAssets.GetDamageVisualEfectTime);
                return;
            }

            _onDamageVisualEffectInProgress = true;

            foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
            {
                List<Material> materials = new();
                renderer.GetSharedMaterials(materials);

                int originalMaterialsCount = materials.Count;
                for (int i = 0; i < originalMaterialsCount; i++)
                {
                    materials.Insert(0, _globalAssets.GetDamageMaterial);
                }

                renderer.sharedMaterials = materials.ToArray();
            }

            Invoke(nameof(RestoreMaterial), _globalAssets.GetDamageVisualEfectTime);
        }

        private void RestoreMaterial()
        {
            if (!_onDamageVisualEffectInProgress)
            {
                Debug.LogError("Cant restore materials, _onDamageVisualEffectInProgress if false");
                return;
            }

            foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
            {
                List<Material> materials = new();
                renderer.GetSharedMaterials(materials);
                int originalMaterialsCount = materials.Count/2;
                for (int i = 0; i < originalMaterialsCount; i++)
                {
                    materials.RemoveAt(0);  
                }
                renderer.sharedMaterials = materials.ToArray();
            }

            _onDamageVisualEffectInProgress = false;
        }
    }
}
