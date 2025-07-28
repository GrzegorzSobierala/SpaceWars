using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.Combat;
using Game.Management;
using Game.Testing;
using TMPro;

namespace Game.Player.Ship
{
    public abstract class HullModuleBase : HullBase , IModule
    {
        public Transform GunSpot => _gunSpot;
        public Transform BridgeSpot => _bridgeSpot;
        public Transform SpecialGunSpot => _specialGunSpot;

        public float CurrentHp => _currentHp; 

        [Inject] protected List<DamageHandlerBase> _damageHandlers;
        [Inject] protected PlayerManager _playerManager;
        [Inject] protected TestingSettings _testingSettings;
        [Inject] protected GlobalAssets _globalAssets;

        [SerializeField] protected Transform _gunSpot;
        [SerializeField] protected Transform _bridgeSpot;
        [SerializeField] protected Transform _specialGunSpot;
        [SerializeField] protected float _baseHp = 1f;
        [SerializeField] protected float _immunityAfterDamageTime = 1.5f;

        protected float _maxHp;
        protected float _currentHp;

        private bool _onDamageVisualEffectInProgress = false;
        private float _lastTakeDamagTime = 0f;

        protected abstract void Defeated();

        protected virtual void OnDestroy()
        {
            foreach (var handler in _damageHandlers)
            {
                if (handler != null)
                    continue;

                handler.Unsubscribe(GetHit);
            }
        }

        public HullModuleBase Instatiate(Transform parent, DiContainer container)
        {
            GameObject hullGM = container.InstantiatePrefab(this, parent);
            HullModuleBase hull = hullGM.GetComponent<HullModuleBase>();

            hull.transform.localPosition = transform.localPosition;
            hull.transform.localRotation = transform.localRotation;

            hull.SetStartHP();

            foreach (var handler in hull._damageHandlers)
            {
                handler.Subscribe(hull.GetHit);
            }

            return hull;
        }

        public bool IsUpgradeAddable(IUpgrade upgrade)
        {
            throw new System.NotImplementedException();
        }

        public bool TryAddUpgrade(IUpgrade upgrade)
        {
            throw new System.NotImplementedException();
        }

        protected void ChangeCurrentHp(float hpChange)
        {
            if (_currentHp == 0)
                return;

            if(hpChange < 0)
            {
                if (Time.time - _lastTakeDamagTime < _immunityAfterDamageTime)
                    return;

                OnDamageVisualEffect();
                _lastTakeDamagTime = Time.time;
            }

            float newCurrentHp = _currentHp + hpChange;

            _currentHp = Mathf.Clamp(newCurrentHp, 0, float.MaxValue);

            if (_currentHp == 0)
            {
                GetDefeated();
                return;
            }
        }

        public void SetStartHP()
        {
            _maxHp = _baseHp;
            _currentHp = _baseHp;

            DEBUG_TrySetHp(_testingSettings.PlayerHp);
        }

        public void DEBUG_TrySetHp(string hpString)
        {
            if (string.IsNullOrEmpty(hpString))
                return;

            if(float.TryParse(hpString,  out float hp))
            {
                _currentHp = hp;   
            }
            else
            {
                Debug.LogError($"Can't parse {nameof(hpString)}: {hpString} to float");
            }
        }

        private void GetDefeated()
        {
            Defeated();
            RestoreMaterial();
            _playerManager.OnPlayerDied?.Invoke();
        }

        private void OnDamageVisualEffect()
        {
            if (_onDamageVisualEffectInProgress)
            {
                CancelInvoke(nameof(RestoreMaterial));
                Invoke(nameof(RestoreMaterial), _immunityAfterDamageTime);
                return;
            }

            _onDamageVisualEffectInProgress = true;

            foreach (var renderer in _body.GetComponentsInChildren<MeshRenderer>(false))
            {
                if (renderer.TryGetComponent(out TextMeshPro _))
                    continue;

                List<Material> materials = new();
                renderer.GetSharedMaterials(materials);

                int originalMaterialsCount = materials.Count;
                for (int i = 0; i < originalMaterialsCount; i++)
                {
                    materials.Insert(0, _globalAssets.ImmunityMaterial);
                }

                renderer.sharedMaterials = materials.ToArray();
            }

            Invoke(nameof(RestoreMaterial), _immunityAfterDamageTime);
        }

        private void RestoreMaterial()
        {
            if (!_onDamageVisualEffectInProgress)
            {
                return;
            }

            foreach (var renderer in _body.GetComponentsInChildren<MeshRenderer>(false))
            {
                if (renderer.TryGetComponent(out TextMeshPro _))
                    continue;

                List<Material> materials = new();
                renderer.GetSharedMaterials(materials);
                int originalMaterialsCount = materials.Count / 2;
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
