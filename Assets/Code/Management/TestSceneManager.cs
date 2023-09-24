using Game.Input.System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game
{
    public class TestSceneManager : MonoBehaviour
    {
        [Inject] private InputProvider _inputProvider;

        private void Awake()
        {
            _inputProvider.PlayerControls.Gameplay.Enable();
        }
    }
}
