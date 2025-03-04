using Game.Management;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Game.Room.Events
{
    [RequireComponent(typeof(Collider2D))]
    public class ObjectEnterEventHandler : MonoBehaviour
    {
        [Inject] private PlayerManager _playerManager;

        [Header("If null set target to player")]
        [SerializeField] private Rigidbody2D _target;
        [SerializeField] private UnityEvent OnBodyEnter;

        private bool _wasTriggered = false;

        private void Awake()
        {
            if (!_target)
            {
                _target = _playerManager.PlayerBody;
            }

            SafeChecks();
        }

        private void OnTriggerEnter2D(Collider2D trigger)
        {
            TryTrigger(trigger);
        }

        private void TryTrigger(Collider2D trigger)
        {
            if (_wasTriggered)
                return;

            if (_target != trigger.attachedRigidbody)
                return;

            OnBodyEnter.Invoke();
            _wasTriggered = true;
            gameObject.SetActive(false);
        }

        private void SafeChecks()
        {
            Collider2D collider = GetComponent<Collider2D>();

            if (!collider.isTrigger)
            {
                Debug.LogError("Collider is not trigger", this);
            }
        }
    }
}
