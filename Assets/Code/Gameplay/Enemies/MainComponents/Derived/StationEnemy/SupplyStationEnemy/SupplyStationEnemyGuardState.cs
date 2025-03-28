using Game.Physics;
using System.Collections.Generic;
using Zenject;

namespace Game.Room.Enemy
{
    public class SupplyStationEnemyGuardState : EnemyGuardStateBase
    {
        [Inject] private List<FieldOfViewEntity> _views;

        protected override void OnEnterState()
        {
            base.OnEnterState();
            SubscribeToFOVs(_views);
        }

        protected override void OnExitState()
        {
            base.OnExitState();
            UnubscribeToFOVs(_views);
        }
    }
}
