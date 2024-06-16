namespace Game.Player
{
    public interface IGun
    {
        public bool IsGunReadyToShoot { get; }

        public abstract void TryShoot();
    }
}
