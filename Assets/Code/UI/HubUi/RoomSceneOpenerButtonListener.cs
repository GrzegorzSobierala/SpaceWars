using Game.Management;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Hub.Ui
{
    [RequireComponent(typeof(Button))]
    public class RoomSceneOpenerButtonListener : MonoBehaviour
    {
        [Inject] GameSceneManager _gameSceneManager;
        [SerializeField, Scene] string _roomName;

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
            _gameSceneManager.LoadRoom(_roomName);
        }
    }
}
