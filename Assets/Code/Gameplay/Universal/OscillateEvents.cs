using UnityEngine;
using UnityEngine.Events;

namespace Game.Universal
{
    public class OscillateEvents : MonoBehaviour
    {
        [SerializeField] private float _oscilateTime = 0.5f;
        [Space]
        [SerializeField] private UnityEvent _oscillateAction1;
        [SerializeField] private UnityEvent _oscillateAction2;

        private float _lastOscilateTime = 0.0f;
        private bool _isWaitingForOscillation1 = false;

        private void Start()
        {
            _oscillateAction1?.Invoke();
        }

        private void Update()
        {
            if (_lastOscilateTime + _oscilateTime < Time.time)
            {
                if(_isWaitingForOscillation1)
                {
                    _oscillateAction1?.Invoke();
                }
                else
                {
                    _oscillateAction2?.Invoke();
                }

                _isWaitingForOscillation1 = !_isWaitingForOscillation1;
                
                _lastOscilateTime = Time.time;
            }
        }

    }
}
