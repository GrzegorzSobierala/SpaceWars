namespace Game.Player.Ship
{
    public interface IModule
    {
        public bool TryAddUpgrade(IUpgrade upgrade);

        public bool IsUpgradeAddable(IUpgrade upgrade);
    }
}
