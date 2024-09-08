using Game.Management;
using UnityEngine;
using Zenject;
using UnityEngine.SceneManagement;
using Game.Utility.Globals;
using System;
using Game.Room.Enemy;
using System.Collections.Generic;

namespace Game.Room
{
    public class PlayerSceneManager : MonoBehaviour
    {
        public Action OnRoomMainObjectiveCompleted;

        [Inject] private PlayerManager _playerManager;
        [Inject] private ZenjectSceneLoader _sceneLoader;

        private List<EnemyBase> _roomEnemies;
        private RoomManager _currentRoomManager;

        public List<EnemyBase> RoomEnemies => _roomEnemies;
        public RoomManager CurrentRoomManager => _currentRoomManager;

        public void Load()
        {
            if (!SceneManager.GetSceneByName(Scenes.CargoTestRoom).isLoaded)
            {
                _sceneLoader.LoadScene(Scenes.CargoTestRoom, LoadSceneMode.Additive);
            }
        }

        public void RestartRoom()
        { 
            if(SceneManager.GetSceneByName(Scenes.CargoTestRoom).isLoaded)
            {
                SceneManager.UnloadSceneAsync(Scenes.CargoTestRoom);
            }

            _sceneLoader.LoadScene(Scenes.CargoTestRoom, LoadSceneMode.Additive);
            _playerManager.PlayerBody.position = Vector2.zero;
            _playerManager.PlayerBody.rotation = 0;
        }

        //_container.Resolve<ZenjectSceneLoader>().LoadSceneAsync("RoomTesting",
        //        UnityEngine.SceneManagement.LoadSceneMode.Additive, (container) =>
        //        {
        //    container.BindInstance(_playerManager);
        //}
        //    );

        public void SetListOfRoomEnemies(List<EnemyBase> enemies)
        {
            _roomEnemies = enemies;
        }

        public void SetCurrentRoomManager(RoomManager roomManager)
        {
            _currentRoomManager = roomManager;
        }
    }
}
