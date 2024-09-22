using Game.Input.System;
using UnityEngine;
using Zenject;

namespace Game.Management
{
    public class HubSceneManager : MonoBehaviour
    {
        [Inject] private InputProvider _input;

        private void Start()
        {
            SetUpScene();
        }

        private void SetUpScene()
        {
            _input.SwitchActionMap(_input.PlayerControls.Ui);
        }
    }
}
