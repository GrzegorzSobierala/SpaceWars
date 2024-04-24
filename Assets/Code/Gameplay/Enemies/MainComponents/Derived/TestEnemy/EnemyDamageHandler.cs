using Game.Combat;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class EnemyDamageHandler : DamageHandlerBase
    {
        public bool IsEnemyInGuardState => _stateMachine.CurrentState is EnemyGuardStateBase;

        [Inject] private EnemyStateMachineBase _stateMachine;

        protected override DamageData ModifyDamage(DamageData damage)
        {
            return damage;
        }
    }
}
