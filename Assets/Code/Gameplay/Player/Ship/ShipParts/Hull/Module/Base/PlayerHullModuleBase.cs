using Game.Combat;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public abstract class PlayerHullModuleBase : PlayerHullBase , IModule
    { 
        [Inject] private DiContainer _container;

        [SerializeField] protected Transform _gunSpot;
        [SerializeField] protected Transform _viewfinderSpot;

        public Transform GunSpot => _gunSpot;
        public Transform ViewfinderSpot => _viewfinderSpot;

        public PlayerHullModuleBase Instatiate(Transform parent, DiContainer container)
        {
            GameObject hullGM = container.InstantiatePrefab(this, parent);
            PlayerHullModuleBase hull = hullGM.GetComponent<PlayerHullModuleBase>();

            hull.transform.localPosition = transform.localPosition;
            hull.transform.localRotation = transform.localRotation;

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
    }
}
