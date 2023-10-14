using Cinemachine;
using UnityEngine;
using Zenject;

namespace Game.Player.VirtualCamera
{
    public class VirtualCameraController : MonoBehaviour
    {
        [Inject] private Rigidbody2D _body;
        [Inject] private CinemachineVirtualCamera _vCamera;
        [Inject] private PlayerEventsHandler _playerController;

        [SerializeField] private float _shakeStrenght = 0.2f;

        private CinemachineFramingTransposer _transposer;
        private CinemachineImpulseSource _impulseSource;

        private void Start()
        {
            Init();
        }

        private void OnEnable()
        {
            _playerController.OnCollisionEnter += ShakeCamera;
        }

        private void OnDisable()
        {
            _playerController.OnCollisionEnter -= ShakeCamera;
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

        public void ShakeCamera(Collision2D collision)
        {
            Vector2 shakeVector = new Vector2( -collision.relativeVelocity.y ,collision.relativeVelocity.x);

            _impulseSource.GenerateImpulse(shakeVector * _shakeStrenght);
        }
    }
}
