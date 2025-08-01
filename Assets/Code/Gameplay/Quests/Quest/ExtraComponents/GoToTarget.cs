using Game.Management;
using Game.Player.Ui;
using System;
using UnityEngine;
using Zenject;

namespace Game.Objectives
{
    [RequireComponent(typeof(Collider2D))]
    public class GoToTarget : MonoBehaviour
    {
        [Inject] private PlayerManager _playerManager;
        [Inject] private MissionPoinerUi _missionPoinerUi;

        public event Action OnPlayerReachedTarget;

        [Header("If null set target to player")]
        [SerializeField] private Rigidbody2D _moveToTarget;

        private bool _wasTriggered = false;

        private void Awake()
        {
            if(!_moveToTarget)
            {
                _moveToTarget = _playerManager.PlayerBody;
            }

            SafeChecks();
        }

        private void Start()
        {
            _missionPoinerUi.SetCurrentTarget(transform);
        }

        private void OnTriggerEnter2D(Collider2D trigger)
        {
            TryTrigger(trigger);
        }

        private void TryTrigger(Collider2D trigger)
        {
            if (_wasTriggered) 
                return;

            if (_moveToTarget != trigger.attachedRigidbody)
                return;

            OnPlayerReachedTarget.Invoke();
            _wasTriggered = true;
        }

        private void SafeChecks()
        {
            Collider2D collider = GetComponent<Collider2D>();
            
            if(!collider.isTrigger)
            {
                Debug.LogError("Collider is not trigger", this);
            }
        }
    }
}
