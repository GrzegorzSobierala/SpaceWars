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
        public Action OnRoomMainObjectiveCompleted;

        [Inject] private GameSceneManager _gameSceneManager;

        private List<EnemyBase> _roomEnemies;
        private RoomManager _currentRoomManager;

        public List<EnemyBase> RoomEnemies => _roomEnemies;
        public RoomManager CurrentRoomManager => _currentRoomManager;

        public void SetListOfRoomEnemies(List<EnemyBase> enemies)
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
    }
}
