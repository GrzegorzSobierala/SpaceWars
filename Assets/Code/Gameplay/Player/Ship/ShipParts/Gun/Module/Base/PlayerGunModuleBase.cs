using Game.Input.System;
using ModestTree;
using UnityEngine;
using UnityEngine.Windows;
using Zenject;

namespace Game.Player.Ship
{
    public abstract class PlayerGunModuleBase : PlayerGunBase , IModule
    {
        [Inject] protected Rigidbody2D _body;

        [Inject] private InputProvider _input;

        [SerializeField] protected ShootableObjectBase _shootableObjectPrefab;
        [SerializeField] protected float _cooldown = 1f;

        protected ShootableObjectBase _shootableObjectPrototype;
        protected float _lastShotTime = 0f;

        protected PlayerControls.GameplayActions Input => _input.PlayerControls.Gameplay;

        protected void Awake()
        {
            if(_shootableObjectPrefab == null)
            {
                Debug.LogError("_shootableObjectPrefab is null, cant create prototype");
            }

            _shootableObjectPrototype = _shootableObjectPrefab.CreateCopy();
            _shootableObjectPrototype.transform.SetParent(transform);
            _shootableObjectPrototype.gameObject.name = _shootableObjectPrefab.gameObject.name + "(Prototype)";
        }

        public PlayerGunModuleBase Instatiate(Transform parent, DiContainer container)
        {
            GameObject gunGM = container.InstantiatePrefab(this, parent);
            PlayerGunModuleBase gun = gunGM.GetComponent<PlayerGunModuleBase>();

            gun.transform.localPosition = transform.localPosition;
            gun.transform.localRotation = transform.localRotation;

            return gun;
        }

        public bool TryAddUpgrade(IUpgrade upgrade)
        {
            throw new System.NotImplementedException();
        }

        public bool IsUpgradeAddable(IUpgrade upgrade)
        {
            throw new System.NotImplementedException();
        }
    }
}
