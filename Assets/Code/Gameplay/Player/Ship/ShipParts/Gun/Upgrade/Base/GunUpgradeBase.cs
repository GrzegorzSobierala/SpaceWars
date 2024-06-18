namespace Game.Player.Ship
{
    public abstract class GunUpgradeBase : GunBase, IUpgrade
    {
        public abstract IUpgrade Instatiate();
    }
}
