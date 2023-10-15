using Game.Player.Upgrade;

namespace Game.Player.Modules
{
    public abstract class PlayerGunBase : UpgradableObjectBase , IGun
    {
        public abstract void Shoot();

        public override bool TryAddUpgrade(UpgradeBase upgrade)
        {
            throw new System.NotImplementedException();
        }
    }
}
