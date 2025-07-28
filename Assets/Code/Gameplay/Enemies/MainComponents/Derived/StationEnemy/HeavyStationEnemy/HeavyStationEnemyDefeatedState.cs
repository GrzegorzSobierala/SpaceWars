namespace Game.Room.Enemy
{
    public class HeavyStationEnemyDefeatedState : EnemyDefeatedStateBase
    {
        protected override void OnEnterState()
        {
            base.OnEnterState();

            Destroy(_enemyBase.gameObject);
        }
    }
}
