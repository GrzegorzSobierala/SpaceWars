using UnityEngine;
using Game.Player.Upgrade;
using Zenject;

namespace Game.Player.Modules
{
    public abstract class PlayerGunBase : UpgradableObjectBase , IGun
    {
        public abstract void Shoot();

        public PlayerGunBase Instatiate(Transform parent, DiContainer container)
        {
            GameObject gunGM = container.InstantiatePrefab(this, parent);
            PlayerGunBase gun = gunGM.GetComponent<PlayerGunBase>();

            gun.transform.localPosition = transform.localPosition;
            gun.transform.localRotation = transform.localRotation;

            return gun;
        }

        public override bool TryAddUpgrade(UpgradeBase upgrade)
        {
            throw new System.NotImplementedException();
        }
    }
}
