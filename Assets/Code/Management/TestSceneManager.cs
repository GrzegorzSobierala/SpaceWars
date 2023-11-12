using Game.Input.System;
using Game.Management;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.SceneManagement;

namespace Game.Room
{
    public class TestSceneManager : MonoBehaviour
    {
        [Inject] private InputProvider _inputProvider;
        [Inject] private PlayerManager _playerManager;
        [Inject] private ZenjectSceneLoader _sceneLoader;
        
        ZenjectSceneLoader sceneLoader;

        //private void Awake()
        //{
        //    _inputProvider.PlayerControls.Gameplay.Enable();
        //}

        public void Load()
        {
            if(!SceneManager.GetSceneByName("RoomTesting").isLoaded)
            {
                _sceneLoader.LoadScene("RoomTesting", LoadSceneMode.Additive);
            }
        }

        public void RestartRoom()
        { 
            if(SceneManager.GetSceneByName("RoomTesting").isLoaded)
            {
                SceneManager.UnloadSceneAsync("RoomTesting");
            }

            _sceneLoader.LoadScene("RoomTesting", LoadSceneMode.Additive);
            _playerManager.PlayerBody.position = Vector2.zero;
            _playerManager.PlayerBody.rotation = 0;
        }

        //_container.Resolve<ZenjectSceneLoader>().LoadSceneAsync("RoomTesting",
        //        UnityEngine.SceneManagement.LoadSceneMode.Additive, (container) =>
        //        {
        //    container.BindInstance(_playerManager);
        //}
        //    );
    }
}
