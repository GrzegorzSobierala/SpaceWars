using Game.Input.System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Game.Player.Ship
{
    public class GunManager : MonoBehaviour
    {
        [Inject] private InputProvider _input;
        [Inject] private ModuleHandler _moduleHandler;

        private bool _isCurrentGunMainGun = true;

        private IGun CurrentGun
        {
            get
            {
                if (_isCurrentGunMainGun)
                    return _moduleHandler.CurrentGun;
                else
                    return _moduleHandler.CurrentSpecialGun;
            }
        }

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
        }

        private void TryShootCurrentGun()
        {
            if (!_moduleHandler.CurrentGun.IsGunReadyToShoot)
                return;

            if (!_moduleHandler.CurrentSpecialGun.IsGunReadyToShoot)
                return;

            if(_isCurrentGunMainGun)
            {
                _moduleHandler.CurrentGun.TryShoot();
            }
            else
            {
                _moduleHandler.CurrentSpecialGun.TryShoot();
            }
        }

        private void OnSwitchGunInput(InputAction.CallbackContext _)
        {
            SwitchCurrentGun();
        }

        private void SwitchCurrentGun()
        {
            _isCurrentGunMainGun = !_isCurrentGunMainGun;
        }
    }
}
