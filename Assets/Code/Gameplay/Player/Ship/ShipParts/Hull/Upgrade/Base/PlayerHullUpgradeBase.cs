namespace Game.Player.Ship
{
    public abstract class PlayerHullUpgradeBase : PlayerHullBase, IUpgrade
    {
        public abstract IUpgrade Instatiate();
    }
}
