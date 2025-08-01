using Game.Combat;
using Game.Input.System;
using Game.Management;
using Game.Room;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public abstract class SpecialGunModuleBase : SpecialGunBase, IModule
    {
        [Inject] protected Rigidbody2D _body;
        [Inject] protected PlayerManager _playerManager;
        [Inject] protected PlayerSceneManager _playerSceneManager;
        [Inject] private InputProvider _input;

        [SerializeField] private ShootableObjectBase _shootableObjectPrefab;
        [SerializeField] protected float _cooldown = 1f;

        protected ShootableObjectBase _shootableObjectPrototype;
        protected float _lastShotTime = 0f;

        protected PlayerControls.GameplayActions Input => _input.PlayerControls.Gameplay;
        protected Transform BulletParent => _playerSceneManager.CurrentRoomManager.
           PlayerObjectsParent.transform;

        protected virtual void Awake()
        {
            if (_shootableObjectPrefab == null)
            {
                Debug.LogError("_shootableObjectPrefab is null, cant create prototype");
            }

            _shootableObjectPrototype = _shootableObjectPrefab.CreateCopy(_body.gameObject, transform);
            _shootableObjectPrototype.gameObject.name = _shootableObjectPrefab.gameObject.name + "(Prototype)";
        }

        public SpecialGunModuleBase Instatiate(Transform parent, DiContainer container)
        {
            GameObject gunGM = container.InstantiatePrefab(this, parent);
            SpecialGunModuleBase gun = gunGM.GetComponent<SpecialGunModuleBase>();

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
