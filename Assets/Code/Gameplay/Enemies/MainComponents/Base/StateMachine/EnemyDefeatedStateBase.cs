using Game.Objectives;
using System;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Game.Room.Enemy
{
    public abstract class EnemyDefeatedStateBase : EnemyStateBase, IDefeatedCallback
    {
        public event Action OnDefeated;

        [Inject] protected EnemyBase _enemyBase;

        [SerializeField] protected UnityEvent OnDestroyEvent;

        public Transform MainTransform => _enemyBase.transform;

        protected override void OnEnterState()
        {
            OnDefeated?.Invoke();
        }

        protected override void OnExitState()
        {

        }

        protected virtual void OnDestroy()
        {
            OnDestroyEvent?.Invoke();
        }
    }
}
