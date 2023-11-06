using Game.Combat;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public abstract class HullModuleBase : HullBase , IModule
    { 
        [Inject] private DiContainer _container;

        [SerializeField] protected Transform _gunSpot;
        [SerializeField] protected Transform _bridgeSpot;

        public Transform GunSpot => _gunSpot;
        public Transform BridgeSpot => _bridgeSpot;

        public HullModuleBase Instatiate(Transform parent, DiContainer container)
        {
            GameObject hullGM = container.InstantiatePrefab(this, parent);
            HullModuleBase hull = hullGM.GetComponent<HullModuleBase>();

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
