using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public abstract class EnemyStateBase : MonoBehaviour
    {
        [Inject] protected EnemyBase _enemy;

        public void EnterState()
        {
            enabled = true;
            OnEnterState();
        }

        public void ExitState()
        {
            enabled = false;
            OnExitState();
        }

        protected abstract void OnEnterState();

        protected abstract void OnExitState();
    }
}
