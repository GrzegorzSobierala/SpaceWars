using Game.Objectives;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Room.Enemy
{
    public abstract class EnemyDefeatedStateBase : EnemyStateBase, IDefeatedCallback
    {
        public event Action onDefeated;

        [SerializeField] protected UnityEvent OnDestroyEvent;

        protected override void OnEnterState()
        {
            onDefeated?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            OnDestroyEvent?.Invoke();
        }
    }
}
