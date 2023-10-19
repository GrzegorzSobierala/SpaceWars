using Game.Combat;
using Game.Player.Upgrade;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Player.Modules
{
    public abstract class PlayerHullBase : UpgradableObjectBase, IHittable 
    { 
        [Inject] private DiContainer _container;

        [SerializeField] protected Transform _gunSpot;

        public Transform GunSpot => _gunSpot;

        public virtual void OnHit() { }

        public override bool TryAddUpgrade(UpgradeBase upgrade)
        {
            throw new System.NotImplementedException();
        }

        public PlayerHullBase Instatiate(Transform parent, DiContainer container)
        {
            GameObject hullGM = container.InstantiatePrefab(this, parent);
            PlayerHullBase hull = hullGM.GetComponent<PlayerHullBase>();

            hull.transform.localPosition = transform.localPosition;
            hull.transform.localRotation = transform.localRotation;

            return hull;
        }
    }
}
