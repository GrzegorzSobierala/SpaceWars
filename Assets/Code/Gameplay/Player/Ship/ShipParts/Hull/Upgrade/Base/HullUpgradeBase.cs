namespace Game.Player.Ship
{
    public abstract class HullUpgradeBase : HullBase, IUpgrade
    {
        public abstract IUpgrade Instatiate();
    }
}
