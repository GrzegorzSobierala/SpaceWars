using UnityEngine;
using UnityEngine.Events;

namespace Game.Room.Enemy
{
    public abstract class EnemyDefeatedStateBase : EnemyStateBase
    {
        [SerializeField] protected UnityEvent OnDestroyEvent;

        protected virtual void OnDestroy()
        {
            OnDestroyEvent?.Invoke();
        }
    }
}
