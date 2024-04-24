using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Game.Room.Enemy
{
    public class CargoEnemyStateMachine : EnemyStateMachineBase , IDocking
    {
        [Inject] private NavMeshAgent _agent;
        [Inject] private Rigidbody2D _body;
        [Inject] private EnemyMovementBase _movement;

        [SerializeField] private DockPlace _mainDock;
        [SerializeField] private DockPlace _suplayDock;
        [SerializeField] private float _distanceBeforeDock = 50;
        [SerializeField] private float _inDockTime = 12;

        private DockPlace _targetDock ;

        public Rigidbody2D Body => _body;

        public float DistanceBeforeDock => _distanceBeforeDock;

        protected override void Start()
        {
            base.Start();

            _targetDock = _mainDock;
            GoToTargetStation();
            _movement.SubscribeOnAchivedTarget(() => _targetDock.StartDocking(this));
        }

        public void OnStartDocking()
        {
            _agent.enabled = false;
            _movement.StopMoving();
        }

        public void OnEndDocking()
        {
            StartCoroutine(WaitAndUndock());
        }

        public void OnStartUnDocking()
        {
        }

        public void OnEndUnDocking()
        {
            _agent.enabled = true;

            _targetDock = _targetDock == _mainDock ? _suplayDock : _mainDock;
            GoToTargetStation();
        }

        private void GoToTargetStation()
        {
            _movement.StartGoingTo(_targetDock.DockingPoint.position + _targetDock.DockingPoint.up * _distanceBeforeDock);
        }

        private IEnumerator WaitAndUndock()
        {
            yield return new WaitForSeconds(_inDockTime);

            _targetDock.StartUnDocking(this);
        }
    }
}
