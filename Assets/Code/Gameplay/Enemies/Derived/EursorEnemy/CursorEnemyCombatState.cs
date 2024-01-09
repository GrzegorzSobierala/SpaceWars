using Game.Management;
using Zenject;

namespace Game.Room.Enemy
{
    public class CursorEnemyCombatState : EnemyCombatStateBase
    {
        [Inject] private EnemyGunBase _gun;
        [Inject] private EnemyMovementBase _movement;
        [Inject] private PlayerManager _playerManager;

        protected override void OnEnterState()
        {

        }

        protected override void OnExitState()
        {

        }
    }
}
