using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Room.Enemy
{
    public class CursorEnemyGuardState : EnemyGuardStateBase
    {
        [Inject] private List<EnemyFieldOfView> _views;

        [SerializeField] private List<Transform> _guardPoints;

        protected virtual void Awake()
        { 
            Initialize();
        }

        protected override void OnEnterState()
        {
            base.OnEnterState();
            Subscribe();
        }

        protected override void OnExitState()
        {
            base.OnExitState();
            Unubscribe();
        }

        private void Initialize()
        {
            if (_guardPoints.Count < 2)
            {
                //Debug.LogError("There need to be at least 2 guard points");
            }
        }

        private void Subscribe()
        {
            foreach (var view in _views)
            {
                view.OnTargetFound += OnPlayerFind;
            }
        }

        private void Unubscribe()
        {
            foreach (var view in _views)
            {
                view.OnTargetFound -= OnPlayerFind;
            }
        }

        private void OnPlayerFind(GameObject foundTarget)
        {
            _stateMachine.SwitchToCombatState();
        }
    }
}
