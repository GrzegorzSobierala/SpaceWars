using Game.Input.System;
using Game.Management;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game
{
    public class TestSceneManager : MonoBehaviour
    {
        [Inject] private InputProvider _inputProvider;
        [Inject] private DiContainer _container;
        [Inject] private PlayerManager _playerManager;

        ZenjectSceneLoader sceneLoader;

        private void Awake()
        {
            _inputProvider.PlayerControls.Gameplay.Enable();
        }

        private void Start()
        {
            //_container.Resolve<ZenjectSceneLoader>().LoadSceneAsync("RoomTesting", 
            //    UnityEngine.SceneManagement.LoadSceneMode.Additive, (container) =>
            //    {
            //        container.BindInstance(_playerManager);
            //    }
            //);
        }
    }
}
