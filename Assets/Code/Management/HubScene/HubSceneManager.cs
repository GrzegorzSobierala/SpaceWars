using Game.Input.System;
using Game.Player.Ui;
using UnityEngine;
using Zenject;

namespace Game.Management
{
    public class HubSceneManager : MonoBehaviour
    {
        [Inject] private InputProvider _input;
        [Inject] private PlayerUiController _playerUiController;
        [Inject] private PlayerManager _playerManager;

        private void Start()
        {
            SetUpScene();
        }

        private void SetUpScene()
        {
            _input.SwitchActionMap(_input.PlayerControls.Ui);
            _playerUiController.SetActive(false);
            _playerManager.SetShipHubMode();
            Time.timeScale = 1;
        }
    }
}
