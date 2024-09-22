using Game.Management;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Ui
{
    [RequireComponent(typeof(Button))]
    public class HubSceneOpenerButtonListener : MonoBehaviour
    {
        [Inject] GameSceneManager _gameSceneManager;

        private Button _roomButton;

        private void Awake()
        {
            _roomButton = GetComponent<Button>();
        }

        private void Start()
        {
            _roomButton.onClick.AddListener(OpenRoom);
        }

        private void OpenRoom()
        {
            _gameSceneManager.LoadHub();
        }
    }
}
