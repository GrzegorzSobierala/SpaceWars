using Game.Input.System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Game.Player.Ship
{
    public abstract class BridgeModuleBase : BridgeBase, IModule
    {
        [Inject] private InputProvider _input;

        private bool IsOffOnToogle = false;

        protected PlayerControls.GameplayActions _Input => _input.PlayerControls.Gameplay;
        protected bool IsAiming { get; private set; } = false;

        private void OnEnable()
        {
            _input.PlayerControls.Gameplay.SwitchGun.started += OnStartedInput;
            _input.PlayerControls.Gameplay.SwitchGun.canceled += OnEndedInput;
        }

        private void OnDisable()
        {
            _input.PlayerControls.Gameplay.SwitchGun.started -= OnStartedInput;
            _input.PlayerControls.Gameplay.SwitchGun.canceled -= OnEndedInput;
        }

        public BridgeModuleBase Instatiate(Transform parent, DiContainer container)
        {
            GameObject viewfinder = container.InstantiatePrefab(this, parent);
            BridgeModuleBase newViewfinder = viewfinder.GetComponent<BridgeModuleBase>();

            newViewfinder.transform.localPosition = transform.localPosition;
            newViewfinder.transform.localRotation = transform.localRotation;

            return newViewfinder;
        }

        public bool TryAddUpgrade(IUpgrade upgrade)
        {
            throw new System.NotImplementedException();
        }

        public bool IsUpgradeAddable(IUpgrade upgrade)
        {
            throw new System.NotImplementedException();
        }

        private void OnStartedInput(InputAction.CallbackContext context)
        {
            if(IsOffOnToogle)
            {
                if(IsAiming)
                {
                    IsAiming = false;
                    OnEndAim();
                }
                else
                {
                    IsAiming = true;
                    OnStartAim();
                }    

            }
            else
            {
                IsAiming = true;
                OnStartAim();
            }
        }

        private void OnEndedInput(InputAction.CallbackContext context)
        {
            if(!IsOffOnToogle)
            {
                IsAiming = false;
                OnEndAim();
            }
        }
    }
}
