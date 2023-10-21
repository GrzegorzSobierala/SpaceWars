namespace Game.Player.Ship
{
    public abstract class PlayerGunUpgradeBase : PlayerGunBase, IUpgrade
    {
        public abstract IUpgrade Instatiate();
    }
}
