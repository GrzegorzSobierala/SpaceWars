using Game.Input.System;
using Game.Utility;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Game.Player.Ship
{
    public class GunManager : MonoBehaviour
    {
        [Inject] private InputProvider _input;
        [Inject] private ModuleHandler _moduleHandler;
        [Inject] private InputManager _inputManager;

        private bool _isCurrentGunMainGun = true;
        private bool _isToggleAim = false;
        private bool _isToggleSwapSteeringOnAim = false;

        public bool IsCurrentGunMainGun => _isCurrentGunMainGun;

        private GunModuleBase Gun => _moduleHandler.CurrentGun;
        private SpecialGunModuleBase SpecialGun => _moduleHandler.CurrentSpecialGun;

        private PlayerControls.GameplayActions GameplayActions => _input.PlayerControls.Gameplay;

        private void Update()
        {
            if (GameplayActions.Shoot.ReadValue<float>() == 1.0f)
            {
                TryShootCurrentGun();
            }

            if (GameplayActions.SwitchGun.WasPerformedThisFrame())
            {
                SwitchCurrentGun();
            }
            
            if(!_isToggleAim && GameplayActions.SwitchGun.WasReleasedThisFrame())
            {
                SwitchCurrentGun();
            }

            if (!_isCurrentGunMainGun)
            {
                UpdateAimSpecialGun();
            }

            if(GameplayActions.ToggleAim.WasPerformedThisFrame())
            {
                SwitchAimType();
            }

            if(GameplayActions.ToggleSwapSteeringOnAim.WasPerformedThisFrame())
            {
                SwitchToggleSwapSteeringOnAim();
            }
        }

        private void TryShootCurrentGun()
        {
            if (!_moduleHandler.CurrentGun.IsGunReadyToShoot)
                return;

            if (!_moduleHandler.CurrentSpecialGun.IsGunReadyToShoot)
                return;

            if(_isCurrentGunMainGun)
            {
                Gun.TryShoot();
            }
            else
            {
                SpecialGun.TryShoot();
            }
        }

        private void OnSwitchGunInput(InputAction.CallbackContext _)
        {
            SwitchCurrentGun();
        }

        private void SwitchCurrentGun()
        {
            _isCurrentGunMainGun = !_isCurrentGunMainGun;

            if (_isToggleSwapSteeringOnAim)
            {
                SwapRotatAndVerdicalMoveInputs();
            }
        }

        private void UpdateAimSpecialGun()
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

            if(_isToggleSwapSteeringOnAim && !_isCurrentGunMainGun)
            {
                SwapRotatAndVerdicalMoveInputs();
            }
        }

        private void SwapRotatAndVerdicalMoveInputs()
        {
            _inputManager.SwapBindings(GameplayActions.MoveLeft.bindings[0],
                    GameplayActions.RotateLeft.bindings[0]);
            _inputManager.SwapBindings(GameplayActions.MoveRight.bindings[0],
                GameplayActions.RotateRight.bindings[0]);
        }
    }
}
