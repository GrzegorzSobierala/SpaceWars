using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public abstract class EnemyStateBase : MonoBehaviour
    {
        public void EnterState()
        {
            gameObject.SetActive(true);
            OnEnterState();
        }

        public void ExitState()
        {
            gameObject.SetActive(false);
            OnExitState();
        }

        protected abstract void OnEnterState();

        protected abstract void OnExitState();
    }
}
