using Cinemachine;
using UnityEngine;
using Zenject;

namespace Game.Player.VirtualCamera
{
    public class VirtualCameraController : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private CinemachineVirtualCamera _vCamera;

        [SerializeField] private float _shakeStrenght = 0.2f;

        private CinemachineFramingTransposer _transposer;
        private CinemachineImpulseSource _impulseSource;

        private void Start()
        {
            Init();
        }

        private void OnEnable()
        {
            _signalBus.Subscribe<OnPlayerCollision2DEnterSignal>(ShakeCamera);
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<OnPlayerCollision2DEnterSignal>(ShakeCamera);
        }

        private void Init()
        {
            _transposer = _vCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            _impulseSource = _vCamera.GetComponent<CinemachineImpulseSource>();

            if (_transposer == null)
            {
                Debug.LogError("No transposer found on virtual camera");
            }
        }

        public void ShakeCamera(OnPlayerCollision2DEnterSignal signal)
        {
            Collision2D collision = signal.Collision;
            Vector2 shakeVector = 
                new Vector2( -collision.relativeVelocity.y ,collision.relativeVelocity.x);

            _impulseSource.GenerateImpulse(shakeVector * _shakeStrenght);
        }
    }
}
