using Game.Input.System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Game.Player.Ship
{
    public abstract class BridgeModuleBase : BridgeBase, IModule
    {
        [Inject] private InputProvider _input;

        protected PlayerControls.GameplayActions _Input => _input.PlayerControls.Gameplay;
        protected bool IsAiming { get; private set; } = false;

        private void OnEnable()
        {
            _input.PlayerControls.Gameplay.Aim.started += CallOnStartAim;
            _input.PlayerControls.Gameplay.Aim.canceled += CallOnEndAim;
        }

        private void OnDisable()
        {
            _input.PlayerControls.Gameplay.Aim.started -= CallOnStartAim;
            _input.PlayerControls.Gameplay.Aim.canceled -= CallOnEndAim;
        }

        public abstract bool IsUpgradeAddable(IUpgrade upgrade);
        public abstract bool TryAddUpgrade(IUpgrade upgrade);

        public BridgeModuleBase Instatiate(Transform parent, DiContainer container)
        {
            GameObject viewfinder = container.InstantiatePrefab(this, parent);
            BridgeModuleBase newViewfinder = viewfinder.GetComponent<BridgeModuleBase>();

            newViewfinder.transform.localPosition = transform.localPosition;
            newViewfinder.transform.localRotation = transform.localRotation;

            return newViewfinder;
        }

        private void CallOnStartAim(InputAction.CallbackContext context)
        {
            IsAiming = true;
            OnStartAim();
        }

        private void CallOnEndAim(InputAction.CallbackContext context)
        {
            IsAiming = false;
            OnEndAim();
        }
    }
}
