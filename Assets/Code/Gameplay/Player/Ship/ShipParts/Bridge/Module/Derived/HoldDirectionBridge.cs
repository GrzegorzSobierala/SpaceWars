using UnityEngine;
using Game.Utility;
using Zenject;
using Game.Player.Control;

namespace Game.Player.Ship
{
    public class HoldDirectionBridge : BridgeModuleBase
    {
        [Inject] private CursorCamera _cursorCamera;

        [SerializeField] private Transform _dierctionMarker;

        private Quaternion _startAimingGunRot;

        private void Start()
        {
            _dierctionMarker.gameObject.SetActive(false);
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
            if (!IsAiming)
            {
                _playerMovement.RotateToCursor();
            }

            _playerMovement.SetVerdicalEnginesPower();
            _playerMovement.SetHorizontalEnginesPower();
            _playerMovement.TryBoost();
        }

        private void UpdateGunAim()
        {
            if (IsAiming)
            {
                AimGun();
            }
        }

        public override void OnStartAim()
        {
            _startAimingGunRot = Gun.transform.localRotation;
            _body.angularVelocity = 0;
            _dierctionMarker.gameObject.SetActive(true);
        }

        public override void OnEndAim()
        {
            Gun.transform.localRotation = _startAimingGunRot;
            _dierctionMarker.gameObject.SetActive(false);
        }

        private void AimGun()
        {
            Vector2 mousePos = _Input.CursorPosition.ReadValue<Vector2>();
            Vector2 aimPoint = _cursorCamera.ScreanPositionOn2DIntersection(mousePos);

            float angleDegrees = Utils.AngleDirected((Vector2)Gun.transform.position, aimPoint);

            Quaternion rotation = Quaternion.Euler(0, 0, angleDegrees);
            Gun.transform.rotation = rotation;
        }
    }
}
