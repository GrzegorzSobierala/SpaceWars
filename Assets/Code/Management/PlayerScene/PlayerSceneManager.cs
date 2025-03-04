using UnityEngine;
using System;
using Game.Room.Enemy;
using System.Collections.Generic;
using Zenject;
using Game.Management;

namespace Game.Room
{
    public class PlayerSceneManager : MonoBehaviour
    {
        public event Action OnEndRoom;

        [Inject] private GameSceneManager _gameSceneManager;

        private HashSet<EnemyBase> _roomEnemies;
        private RoomManager _currentRoomManager;

        public HashSet<EnemyBase> RoomEnemies => _roomEnemies;
        public RoomManager CurrentRoomManager => _currentRoomManager;

        public void SetRoomEnemies(HashSet<EnemyBase> enemies)
        {
            _roomEnemies = enemies;
        }

        public void SetCurrentRoomManager(RoomManager roomManager)
        {
            _currentRoomManager = roomManager;
        }

        public void RestartRoom(Action onEnd = null)
        {
            _gameSceneManager.ReloadCurrentRoom(onEnd);
        }

        public void EndRoom()
        {
            OnEndRoom.Invoke();
        }
    }
}
