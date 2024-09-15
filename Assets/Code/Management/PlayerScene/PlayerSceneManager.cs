using UnityEngine;
using System;
using Game.Room.Enemy;
using System.Collections.Generic;
using Game.Input.System;
using Game.Testing;
using Zenject;
using Game.Management;
using System.Runtime.CompilerServices;

namespace Game.Room
{
    public class PlayerSceneManager : MonoBehaviour
    {
        public Action OnRoomMainObjectiveCompleted;

        [Inject] private TestingSettings _testingSettings;
        [Inject] private InputProvider _inputProvider;
        [Inject] private GameSceneManager _gameSceneManager;

        private List<EnemyBase> _roomEnemies;
        private RoomManager _currentRoomManager;

        public List<EnemyBase> RoomEnemies => _roomEnemies;
        public RoomManager CurrentRoomManager => _currentRoomManager;

        private void Start()
        {
            _inputProvider.SetGameplayInput();
        }

        public void SetListOfRoomEnemies(List<EnemyBase> enemies)
        {
            _roomEnemies = enemies;
        }

        public void SetCurrentRoomManager(RoomManager roomManager)
        {
            _currentRoomManager = roomManager;
        }

        public TaskAwaiter RestartRoom()
        {
            return _gameSceneManager.ReloadCurrentRoom();
        }
    }
}
