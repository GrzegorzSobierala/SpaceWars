using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input.System
{
    public class InputProvider : MonoBehaviour
    {
        public PlayerControls PlayerControls => _playerControls;
        private PlayerControls _playerControls;

        private void Awake()
        {
            _playerControls = new PlayerControls();
        }

        public void SwitchActionMap(InputActionMap actionMap)
        {
            foreach (var map in _playerControls.asset.actionMaps)
            {
                if (map == actionMap)
                {
                    map.Enable();
                }
                else
                {
                    map.Disable();
                }
            }  
        }
    }
}
