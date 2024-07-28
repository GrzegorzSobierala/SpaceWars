using Game.Input.System;
using Game.Utility;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Game.Player.Ship
{
    public class GunManager : MonoBehaviour
    {
        public Action OnSwitchToMainGun;
        public Action OnSwitchToSpecialGun;

        [Inject] private InputProvider _input;
        [Inject] private ModuleHandler _moduleHandler;
        [Inject] private PlayerMovement2D _movement2D;

        private bool _isCurrentGunMainGun = true;
        private bool _isToggleAim = false;
        private bool _isToggleSwapSteeringOnAim = false;

        public bool IsCurrentGunMainGun => _isCurrentGunMainGun;

        private GunModuleBase Gun => _moduleHandler.CurrentGun;
        private SpecialGunModuleBase SpecialGun => _moduleHandler.CurrentSpecialGun;

        private PlayerControls.GameplayActions GameplayActions => _input.PlayerControls.Gameplay;

        private void Update()
        {
            UpdateInput();
            UpdateGuns();
        }

        private void UpdateInput()
        {
            if (GameplayActions.ToggleAim.WasPerformedThisFrame())
            {
                SwitchAimType();
            }

            if (GameplayActions.ToggleSwapSteeringOnAim.WasPerformedThisFrame())
            {
                SwitchToggleSwapSteeringOnAim();
            }
        }

        private void UpdateGuns()
        {
            if (GameplayActions.SwitchGun.WasPerformedThisFrame())
            {
                SwitchCurrentGun();
            }

            if (!_isToggleAim && GameplayActions.SwitchGun.WasReleasedThisFrame())
            {
                SwitchCurrentGun();
            }

            if (!_isCurrentGunMainGun)
            {
                AimSpecialGun();
            }

            if (GameplayActions.Shoot.ReadValue<float>() == 1.0f)
            {
                TryShootCurrentGun();
            }
        }

        private void TryShootCurrentGun()
        {
            if(_isCurrentGunMainGun)
            {
                Gun.TryShoot();
            }
            else
            {
                SpecialGun.TryShoot();
            }
        }

        private void SwitchCurrentGun()
        {
            _isCurrentGunMainGun = !_isCurrentGunMainGun;

            if(_isCurrentGunMainGun)
            {
                OnSwitchToMainGun?.Invoke();
            }
            else
            {
                OnSwitchToSpecialGun?.Invoke();
            }

            if (_isToggleSwapSteeringOnAim)
            {
                SwapRotatAndVerdicalMoveInputs();
            }
        }

        private void AimSpecialGun()
        {
            Vector2 mousePos = GameplayActions.CursorPosition.ReadValue<Vector2>();
            Vector2 aimPoint = Utils.ScreanPositionOn2DIntersection(mousePos);

            Vector2 specialGunPos = (Vector2)SpecialGun.transform.position;
            float angleDegrees = Utils.AngleDirected(specialGunPos, aimPoint) - 90f;

            Quaternion rotation = Quaternion.Euler(0, 0, angleDegrees);
            SpecialGun.transform.rotation = rotation;
        }

        private void SwitchAimType()
        {
            _isToggleAim = !_isToggleAim;

            if(!_isToggleAim && !_isCurrentGunMainGun)
            {
                SwitchCurrentGun();
            }
        }

        private void SwitchToggleSwapSteeringOnAim()
        {
            _isToggleSwapSteeringOnAim = !_isToggleSwapSteeringOnAim;
        }

        private void SwapRotatAndVerdicalMoveInputs()
        {
            _movement2D.InverseRotWithVerMove();
        }
    }
}
