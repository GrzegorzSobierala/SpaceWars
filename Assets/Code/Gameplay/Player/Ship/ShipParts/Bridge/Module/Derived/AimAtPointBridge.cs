using Game.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public class AimAtPointBridge : BridgeModuleBase
    {
        [Inject] private CursorCamera _cursorCamera;

        [SerializeField] private Transform _aimMarker;

        private Quaternion _startAimingGunRot;

        private void Awake()
        {
            _aimMarker.gameObject.SetActive(false);
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
            _playerMovement.RotateToCursor();
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

            _aimMarker.SetParent(_playerModuleHandler.transform.parent);
            _aimMarker.gameObject.SetActive(true);

            Vector3 targetPosition = new Vector3(aimPoint.x, aimPoint.y, 0);
            _aimMarker.position = targetPosition;
        }

        public override void OnEndAim()
        {
            _aimMarker.SetParent(transform);
            _aimMarker.gameObject.SetActive(false);
            _aimMarker.localPosition = Vector3.zero;

            Gun.transform.localRotation = _startAimingGunRot;
        }

        private void AimGun()
        {

            Vector2 gunPos = (Vector2)Gun.transform.position;
            float angleDegrees = Utils.AngleDirected(gunPos, _aimMarker.position);

            Quaternion rotation = Quaternion.Euler(0, 0, angleDegrees);
            Gun.transform.rotation = rotation;
        }
    }
}
