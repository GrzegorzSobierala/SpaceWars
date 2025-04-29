namespace Game.Room.Enemy
{
    public interface IGuardStateDetectable
    {
        public bool IsEnemyInGuardState { get; }

        public EnemyBase Enemy { get; }
    }
}
