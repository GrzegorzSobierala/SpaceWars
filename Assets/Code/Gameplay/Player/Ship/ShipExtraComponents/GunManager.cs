using Game.Input.System;
using Game.Utility;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using Zenject;
using static Zenject.CheatSheet;

namespace Game.Player.Ship
{
    public class GunManager : MonoBehaviour
    {
        [Inject] private InputProvider _input;
        [Inject] private ModuleHandler _moduleHandler;

        private bool _isCurrentGunMainGun = true;
        private Quaternion _startAimingGunRot;

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

            if (!_isCurrentGunMainGun)
            {
                UpdateAimSpecialGun();
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

            if(_isCurrentGunMainGun)
            {
                OnEndAimSpecialGun();
            }
            else
            {
                OnStartAimSpecialGun();
            }
        }

        private void OnStartAimSpecialGun()
        {
            //_startAimingGunRot = SpecialGun.transform.localRotation;
        }

        private void OnEndAimSpecialGun()
        {
            //SpecialGun.transform.localRotation = _startAimingGunRot;
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
    }
}
