using Game.Input.System;
using UnityEngine;
using Zenject;

namespace Game.Room
{
    public class RoomManager : MonoBehaviour
    {
        [Inject] private PlayerSceneManager _playerSceneManager;
        [Inject] private PlayerObjectsParent _playerObjectsParent;
        [Inject] private InputProvider _input;

        public PlayerObjectsParent PlayerObjectsParent => _playerObjectsParent;

        private void Start()
        {
            _playerSceneManager.SetCurrentRoomManager(this);
            _input.SwitchActionMap(_input.PlayerControls.Gameplay);
        }
    }
}
