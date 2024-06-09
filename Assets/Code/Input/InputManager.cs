using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Game.Input.System
{
    public class InputManager : MonoBehaviour
    {
        [Inject] private InputProvider _inputProvider;

        private void Start()
        {
            TestSwapMovementBindingsSubscribe();
        }

        public void SwapBindings(InputBinding binding1, InputBinding binding2)
        {
            if (binding1 == binding2)
            {
                Debug.LogError("Bindings are the same, cannot swap.");
                return;
            }

            InputAction action1 = GetInputActionFromBinding(binding1);
            InputAction action2 = GetInputActionFromBinding(binding2);

            if(action1 == null)
            {
                Debug.LogError($"Cannot find action for binding1: {binding1.name}");
                return;
            }

            if (action2 == null)
            {
                Debug.LogError($"Cannot find action for binding2: {binding2.name}");
                return;
            }

            if (action1.actionMap != action2.actionMap) 
            { 
                Debug.LogError("Actions are not in the same map, cannot swap bindings.");
                return;
            }

            var effectivePath1 = binding1.effectivePath;
            var effectivePath2 = binding2.effectivePath;

            InputBinding newOverrideBinding1 = binding1;
            newOverrideBinding1.overridePath = effectivePath2;
            action1.ApplyBindingOverride(0, newOverrideBinding1);

            InputBinding newOverrideBinding2 = binding2;
            newOverrideBinding2.overridePath = effectivePath1;
            action2.ApplyBindingOverride(0, newOverrideBinding2);

            Debug.Log($"Bindings swapped: {binding1.name} <--> {binding2.name}");
        }

        public InputAction GetInputActionFromBinding(InputBinding inputBinding)
        {
            foreach (var actionMap in _inputProvider.PlayerControls.asset.actionMaps)
            {
                foreach (var action in actionMap.actions)
                {
                    foreach (var binding in action.bindings)
                    {
                        if (binding.id == inputBinding.id)
                        {
                            return action;
                        }
                    }
                }
            }

            Debug.Log($"Cant find InputAction with this InputBinding: {inputBinding.name}, returning null");
            return null;
         }

        private void TestSwapMovementBindingsSubscribe()
        {
            var gameplayInput = _inputProvider.PlayerControls.Gameplay;

            gameplayInput.SwapSteering.performed += (InputAction.CallbackContext _) =>
            SwapBindings(gameplayInput.MoveLeft.bindings[0], gameplayInput.RotateLeft.bindings[0]);

            gameplayInput.SwapSteering.performed += (InputAction.CallbackContext _) =>
            SwapBindings(gameplayInput.MoveRight.bindings[0], gameplayInput.RotateRight.bindings[0]);
        }
    }
}
