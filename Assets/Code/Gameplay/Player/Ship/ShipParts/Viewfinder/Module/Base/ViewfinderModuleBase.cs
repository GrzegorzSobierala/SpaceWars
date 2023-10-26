using Game.Input.System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Game.Player.Ship
{
    public abstract class ViewfinderModuleBase : ViewfinderBase, IModule
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

        public ViewfinderModuleBase Instatiate(Transform parent, DiContainer container)
        {
            GameObject viewfinder = container.InstantiatePrefab(this, parent);
            ViewfinderModuleBase newViewfinder = viewfinder.GetComponent<ViewfinderModuleBase>();

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
