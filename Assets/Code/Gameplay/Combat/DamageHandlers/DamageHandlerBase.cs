using Game.Room.Enemy;
using System;
using UnityEngine;
using Zenject;

namespace Game.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class DamageHandlerBase : MonoBehaviour, IHittable, IGuardStateDetectable
    {
        [InjectOptional] private EnemyStateMachineBase _stateMachine;
        [InjectOptional] private EnemyBase _enemy;

        private Action<DamageData> OnGetHit;

        private Collider2D _collider;

        public Collider2D Collider => _collider;
        public EnemyBase Enemy => _enemy; 

        public bool IsEnemyInGuardState
        {
            get
            {
                if (!_stateMachine)
                    return false;

                return _stateMachine.CurrentState is EnemyGuardStateBase;
            }
        }

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }

        public void GetHit(DamageData damage)
        {
            damage = ModifyDamage(damage);
            OnGetHit?.Invoke(damage);
        }

        protected abstract DamageData ModifyDamage(DamageData damage);

        public void Subscribe(Action<DamageData> onGetHit)
        {
            OnGetHit += onGetHit;
        }

        public void Unsubscribe(Action<DamageData> onGetHit)
        {
            OnGetHit -= onGetHit;
        }
    }
}
