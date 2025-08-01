using Game.Input.System;
using Game.Management;
using Game.Combat;
using UnityEngine;
using Zenject;
using Game.Room;

namespace Game.Player.Ship
{
    public abstract class GunModuleBase : GunBase , IModule
    {
        [Inject] protected Rigidbody2D _body;
        [Inject] protected PlayerManager _playerManager;
        [Inject] protected PlayerSceneManager _playerSceneManager;

        [SerializeField] private ShootableObjectBase _shootableObjectPrefab;
        [SerializeField] protected float _cooldown = 1f;

        protected ShootableObjectBase _shootableObjectPrototype;
        protected float _lastShotTime = 0f;

        protected Transform BulletParent => _playerSceneManager.CurrentRoomManager.
            PlayerObjectsParent.transform;

        protected virtual void Awake()
        {
            if(_shootableObjectPrefab == null)
            {
                Debug.LogError("_shootableObjectPrefab is null, cant create prototype");
            }

            _shootableObjectPrototype = _shootableObjectPrefab.CreateCopy(_body.gameObject, transform);
            _shootableObjectPrototype.gameObject.name = _shootableObjectPrefab.gameObject.name + "(Prototype)";
        }

        public GunModuleBase Instatiate(Transform parent, DiContainer container)
        {
            GameObject gunGM = container.InstantiatePrefab(this, parent);
            GunModuleBase gun = gunGM.GetComponent<GunModuleBase>();

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
