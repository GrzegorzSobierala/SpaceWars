using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.Combat;
using Game.Management;

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

        [SerializeField] protected Transform _gunSpot;
        [SerializeField] protected Transform _bridgeSpot;
        [SerializeField] protected Transform _specialGunSpot;
        [SerializeField] protected float _baseHp = 1f;

        protected float _maxHp;
        protected float _currentHp;

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
        }

        public void DEBUG_SetHp(int hp)
        {
            _currentHp = hp;
        }

        private void GetDefeated()
        {
            Defeated();
            _playerManager.OnPlayerDied?.Invoke();
        }
    }
}
