using UnityEngine;

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
    }
}
