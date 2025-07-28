using Game.Input.System;
using Game.Management;
using Game.Player.Ui;
using UnityEngine;
using Zenject;

namespace Game.Room
{
    public class RoomManager : MonoBehaviour
    {
        [Inject] private PlayerSceneManager _playerSceneManager;
        [Inject] private PlayerObjectsParent _playerObjectsParent;
        [Inject] private PlayerUiController _playerUiController;
        [Inject] private PlayerManager _playerManager;
        [Inject] private InputProvider _input;
        [Inject] private TestResetUi _testResetUi;

        public PlayerObjectsParent PlayerObjectsParent => _playerObjectsParent;

        private void Start()
        {
            _playerSceneManager.SetCurrentRoomManager(this);
            _input.SwitchActionMap(_input.PlayerControls.Gameplay);
            _playerUiController.SetActive(true);
            _playerManager.SetShipRoomMode();
            _testResetUi.OpenTutorialOnceForBuild();
        }
    }
}
