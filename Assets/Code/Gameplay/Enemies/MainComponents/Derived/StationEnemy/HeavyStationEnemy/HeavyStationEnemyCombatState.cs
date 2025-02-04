using Game.Management;
using Zenject;

namespace Game.Room.Enemy
{
    public class HeavyStationEnemyCombatState : EnemyCombatStateBase
    {
        [Inject] private EnemyBase _enemyBase;
        [Inject] protected PlayerManager _playerManager;

        protected override void OnEnterState()
        {
            base.OnEnterState();
        }

        protected override void OnExitState()
        {
            base.OnExitState();
        }
    }
}
