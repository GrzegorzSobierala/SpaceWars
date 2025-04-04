using Game.Management;
using Game.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Game.Room.Enemy
{
    public class CargoEnemyStateMachine : EnemyStateMachineBase , IDocking
    {
        public event Func<bool> CanUndock;
        public event Action OnObjectDestroy;

        [Inject] private NavMeshAgent _agent;
        [Inject] private Rigidbody2D _body;
        [Inject] private EnemyMovementBase _movement;
        [Inject] private List<EnemyGunBase> _guns;
        [Inject] private PlayerManager _playerManager;
        
        [SerializeField] private float _distanceBeforeDock = 50;
        [SerializeField] private float _inDockTime = 12;
        [Space]
        [SerializeField] private DockPlace _mainDock;
        [SerializeField] private DockPlace _suplayDock;
        [SerializeField] private GameObject _engineParticles;

        private Vector2 _lastMainDockPos;
        private Vector2 _lastSupplyDockPos;
        private bool _knowMainStationDestroyed = false;
        private bool _knowSupplyStationDestroyed = false;
        private bool _isMainDockTarget = false;

        public Rigidbody2D Body => _body;

        public float DistanceBeforeDock => _distanceBeforeDock;

        protected override void Start()
        {
            base.Start();

            StartMoveToNextTarget();
            _movement.SubscribeOnAchivedTarget(OnAchivedTargetAction);
            UpdateLastDockPositions();
        }

        private void OnDestroy()
        {
            if (GameManager.IsGameQuitungOrSceneUnloading(gameObject))
                return;

            OnObjectDestroy?.Invoke();
        }

        public void OnStartDocking()
        {
            _agent.enabled = false;
            _movement.StopMoving(); 
            _engineParticles.SetActive(false);
            foreach (var gun in _guns)
            {
                gun.Prepare();
            }
        }

        public void OnEndDocking()
        {
            StartCoroutine(WaitAndUndock());
        }

        public void OnStartUnDocking()
        {
            ClearSubscribers();
        }

        public void OnEndUnDocking()
        {
            ChangeTargetAndMove();
        }

        public void OnDockDestroy()
        {
            if(_isMainDockTarget)
            {
                _knowMainStationDestroyed = true;
            }
            else
            {
                _knowSupplyStationDestroyed = true;
            }

            ChangeTargetAndMove();
            StopAllCoroutines();
            ClearSubscribers();
        }

        private void StartMoveToNextTarget()
        {
            UpdateLastDockPositions();

            if (_knowMainStationDestroyed || _knowSupplyStationDestroyed)
            {
                if (_knowMainStationDestroyed && _knowSupplyStationDestroyed)
                {
                    _movement.UnsubscribeOnAchivedTarget(OnAchivedTargetAction);
                    _movement.StartGoingTo(_playerManager.PlayerBody.transform);
                }
                else if (!_knowSupplyStationDestroyed)
                {
                    _movement.UnsubscribeOnAchivedTarget(OnAchivedTargetAction);
                    _movement.StartGoingTo(_playerManager.PlayerBody.transform);
                }
                else if (!_knowMainStationDestroyed)
                {
                    _movement.StartGoingTo(_lastMainDockPos);
                }

                return;
            }
            else
            {
                _movement.StartGoingTo(GetTargetDockPos());
            }
        }

        private IEnumerator WaitAndUndock()
        {
            yield return new WaitForSeconds(_inDockTime);

            yield return new WaitUntil(() => Utils.EvaluateCombinedFunc(CanUndock));

            if(_isMainDockTarget)
            {
                if(!_knowSupplyStationDestroyed)
                {
                    GetCurrentTargetDock().StartUnDocking(this);
                }
            }
            else
            {
                if (!_knowMainStationDestroyed)
                {
                    GetCurrentTargetDock().StartUnDocking(this);
                }
            }
        }

        private void OnAchivedTargetAction()
        {
            if (GetCurrentTargetDock() == null)
            {
                if (_isMainDockTarget)
                {
                    _knowMainStationDestroyed = true;
                }
                else
                {
                    _knowSupplyStationDestroyed = true;
                }
            }

            if (_knowMainStationDestroyed && _knowSupplyStationDestroyed)
            {
                _movement.UnsubscribeOnAchivedTarget(OnAchivedTargetAction);
                _movement.StartGoingTo(_playerManager.PlayerBody.transform);
                return;
            }

            if (_isMainDockTarget && _knowMainStationDestroyed)
            {
                _isMainDockTarget = false;
                _movement.StartGoingTo(_lastSupplyDockPos);
                return;
            }

            if (!_isMainDockTarget && _knowSupplyStationDestroyed)
            {
                _movement.UnsubscribeOnAchivedTarget(OnAchivedTargetAction);
                _movement.StartGoingTo(_playerManager.PlayerBody.transform);
                return;
            }

            GetCurrentTargetDock().StartDocking(this);
        }

        private void UpdateLastDockPositions()
        {
            if(_mainDock)
            {
                _lastMainDockPos = _mainDock.DockingPoint.position +
                    _mainDock.DockingPoint.up * _distanceBeforeDock;
            }
            
            if(_suplayDock)
            {
                _lastSupplyDockPos = _suplayDock.DockingPoint.position +
                    _suplayDock.DockingPoint.up * _distanceBeforeDock;
            }
        }

        private DockPlace GetCurrentTargetDock()
        {
            return _isMainDockTarget ? _mainDock : _suplayDock;
        }

        private Vector2 GetTargetDockPos()
        {
            UpdateLastDockPositions();
            return _isMainDockTarget ? _lastMainDockPos : _lastSupplyDockPos;
        }

        private void ChangeTargetAndMove()
        {
            _agent.enabled = true;
            _isMainDockTarget = !_isMainDockTarget;
            StartMoveToNextTarget();
            _engineParticles.SetActive(true);
        }

        private void ClearSubscribers()
        {
            CanUndock = null;
            OnObjectDestroy = null;
        }
    }
}
