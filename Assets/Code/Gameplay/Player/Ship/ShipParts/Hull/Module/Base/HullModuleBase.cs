using System;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public abstract class HullModuleBase : HullBase , IModule
    {
        public Action<HullModuleBase> OnDefeatAction;

        [SerializeField] protected Transform _gunSpot;
        [SerializeField] protected Transform _bridgeSpot;
        [SerializeField] private readonly float _baseHp = 1f;

        protected float _maxHp;
        protected float _currentHp;

        public Transform GunSpot => _gunSpot;
        public Transform BridgeSpot => _bridgeSpot;

        protected abstract void Defeated();

        public HullModuleBase Instatiate(Transform parent, DiContainer container)
        {
            GameObject hullGM = container.InstantiatePrefab(this, parent);
            HullModuleBase hull = hullGM.GetComponent<HullModuleBase>();

            hull.transform.localPosition = transform.localPosition;
            hull.transform.localRotation = transform.localRotation;

            hull.SetStartHP();

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

            _currentHp = Mathf.Clamp(newCurrentHp, 0, _maxHp);

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

        private void GetDefeated()
        {
            Defeated();
            OnDefeatAction?.Invoke(this);
        }
    }
}
