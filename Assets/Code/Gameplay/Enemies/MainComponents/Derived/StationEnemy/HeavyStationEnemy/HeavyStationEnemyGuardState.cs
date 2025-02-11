using System.Collections.Generic;
using Zenject;

namespace Game.Room.Enemy
{
    public class HeavyStationEnemyGuardState : EnemyGuardStateBase
    {
        [Inject] private List<EnemyFieldOfView> _views;

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
