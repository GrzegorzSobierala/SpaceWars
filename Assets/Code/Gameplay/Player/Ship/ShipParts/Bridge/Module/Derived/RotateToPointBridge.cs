using UnityEngine;
using Zenject;
using Game.Player.Control;
using Game.Utility;

namespace Game.Player.Ship
{
    public class RotateToPointBridge : BridgeModuleBase
    {
        [Inject] private PlayerFallower _playerFallower;
        [Inject] private CursorCamera _cursorCamera;

        [SerializeField] private Transform _rotateMarker;
        [SerializeField] private bool _localMarkerParent;

        private Quaternion _startAimingGunRot;

        private void Awake()
        {
            _rotateMarker.gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            UpdateMovement();
        }

        private void Update()
        {
            UpdateGunAim();
        }

        private void UpdateMovement()
        {
            if (IsAiming)
            {
                Vector3 screenMarkerPos = Camera.main.WorldToScreenPoint(_rotateMarker.position);
                _playerMovement.RotateToPoint(screenMarkerPos);
            }
            else
            {
                _playerMovement.RotateToCursor();
            }

            _playerMovement.VerdicalMove();
            _playerMovement.HorizontalMove();
            _playerMovement.TryBoost();
        }

        private void UpdateGunAim()
        {
            if (!IsAiming)
                return;

            AimGun();
        }

        public override void OnStartAim()
        {
            _startAimingGunRot = Gun.transform.localRotation;

            Vector2 mousePos = _Input.CursorPosition.ReadValue<Vector2>();
            Vector2 aimPoint = _cursorCamera.ScreanPositionOn2DIntersection(mousePos);

            if (_localMarkerParent)
            {
                _rotateMarker.SetParent(_playerFallower.transform);
            }
            else
            {
                _rotateMarker.SetParent(_playerModuleHandler.transform.parent);
            }

            _rotateMarker.gameObject.SetActive(true);

            Vector3 targetPosition = new Vector3(aimPoint.x, aimPoint.y, 0);
            _rotateMarker.position = targetPosition;

            _startAimingGunRot = Gun.transform.localRotation;
        }

        public override void OnEndAim()
        {
            _rotateMarker.SetParent(transform);
            _rotateMarker.gameObject.SetActive(false);
            _rotateMarker.localPosition = Vector3.zero;

            Gun.transform.localRotation = _startAimingGunRot;
        }

        private void AimGun()
        {
            Vector2 mousePos = _Input.CursorPosition.ReadValue<Vector2>();
            Vector2 aimPoint = _cursorCamera.ScreanPositionOn2DIntersection(mousePos);

            Vector2 gunPos = (Vector2)Gun.transform.position;
            float angleDegrees = Utils.AngleDirected(gunPos, aimPoint);

            Quaternion rotation = Quaternion.Euler(0, 0, angleDegrees);
            Gun.transform.rotation = rotation;
        }
    }
}
