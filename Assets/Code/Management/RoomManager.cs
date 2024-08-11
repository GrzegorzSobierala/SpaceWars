using UnityEngine;
using Zenject;

namespace Game.Room
{
    public class RoomManager : MonoBehaviour
    {
        [Inject] private PlayerSceneManager _playerSceneManager;
        [Inject] private PlayerObjectsParent _playerObjectsParent;

        public PlayerObjectsParent PlayerObjectsParent => _playerObjectsParent;

        private void Start()
        {
            _playerSceneManager.SetCurrentRoomManager(this);
        }
    }
}
