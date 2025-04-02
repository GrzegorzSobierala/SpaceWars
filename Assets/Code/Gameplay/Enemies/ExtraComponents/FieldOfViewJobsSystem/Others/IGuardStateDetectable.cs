namespace Game.Room.Enemy
{
    public interface IGuardStateDetectable
    {
        public bool IsEnemyInGuardState { get; }

        public EnemyBase Enemy { get; }
        //public bool IsEnemyInGuardState => _stateMachine.CurrentState is EnemyGuardStateBase;

        //[Inject] private EnemyStateMachineBase _stateMachine;
    }
}
