using UnityEngine;

namespace Game.Room.Enemy
{
    public class CursorEnemyDefeatedState : EnemyDefeatedStateBase
    {
        protected override void OnEnterState()
        {
            Destroy(_enemy.gameObject);
        }

        protected override void OnExitState()
        {
            Debug.LogError($"No OnExitState in {typeof(CursorEnemyDefeatedState)}");
        }
    }
}
