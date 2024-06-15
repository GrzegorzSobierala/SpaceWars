namespace Game.Player.Ship
{
    public abstract class GunUpgradeBase : MainGunBase, IUpgrade
    {
        public abstract IUpgrade Instatiate();
    }
}
