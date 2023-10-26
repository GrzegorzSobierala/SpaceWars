using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public abstract class PlayerGunModuleBase : PlayerGunBase , IModule
    {
        public abstract bool TryAddUpgrade(IUpgrade upgrade);
        public abstract bool IsUpgradeAddable(IUpgrade upgrade);

        public PlayerGunModuleBase Instatiate(Transform parent, DiContainer container)
        {
            GameObject gunGM = container.InstantiatePrefab(this, parent);
            PlayerGunModuleBase gun = gunGM.GetComponent<PlayerGunModuleBase>();

            gun.transform.localPosition = transform.localPosition;
            gun.transform.localRotation = transform.localRotation;

            return gun;
        }

        public virtual void AddUpgrade(PlayerGunUpgradeBase upgrade)
        {
            throw new System.NotImplementedException();
        }
    }
}
