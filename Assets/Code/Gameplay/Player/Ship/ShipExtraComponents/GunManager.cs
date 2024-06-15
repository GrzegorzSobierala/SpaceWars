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

        private bool _isCurrentGunMainGun;

        private PlayerControls.GameplayActions GameplayActions => _input.PlayerControls.Gameplay;

        private void OnEnable()
        {
            GameplayActions.Shoot.performed += TryShootCurrentGun;
            GameplayActions.SwitchGun.performed += OnSwitchGunInput;
        }

        private void OnDisable()
        {
            GameplayActions.Shoot.performed -= TryShootCurrentGun;
            GameplayActions.SwitchGun.performed -= OnSwitchGunInput;
        }

        private void TryShootCurrentGun(InputAction.CallbackContext _)
        {
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
