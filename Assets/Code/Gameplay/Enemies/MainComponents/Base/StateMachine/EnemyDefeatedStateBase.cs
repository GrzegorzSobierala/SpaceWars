using Game.Management;
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
        [Inject] protected GlobalAssets _globalAssets;

        [SerializeField] protected UnityEvent OnDestroyEvent;
        [SerializeField] private bool _enableBasicOnDestroyEffect = true;
        [SerializeField] private float _basicOnDestroyEffectScale = 1f;


        public Transform MainTransform => _enemyBase.transform;

        protected override void OnEnterState()
        {
            OnDefeated?.Invoke();

            if(_enableBasicOnDestroyEffect)
            {
                BasicOnDestroyEffect();
            }
        }

        protected override void OnExitState()
        {

        }

        protected virtual void OnDestroy()
        {
            OnDestroyEvent?.Invoke();
        }

        private void BasicOnDestroyEffect()
        {
            var newGo = Instantiate(_globalAssets.OnEnemyDestroyedEffect, _enemyBase.transform.position,
                _globalAssets.transform.rotation, _enemyBase.transform.parent);

            newGo.transform.localScale *= _basicOnDestroyEffectScale;
        }
    }
}
