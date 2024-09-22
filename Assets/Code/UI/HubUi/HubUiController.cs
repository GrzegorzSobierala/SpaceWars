using Game.Input.System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Game.Hub.Ui
{
    public class HubUiController : MonoBehaviour
    {
        [Inject] private InputProvider _input;

        private Stack<GameObject> openPanels = new();

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void OpenPanel(GameObject toOpen)
        {
            toOpen.SetActive(true);
            openPanels.Push(toOpen);
        }

        public void CloseLastOpenedPanel()
        {
            if(openPanels.Count == 0)
                return;

            openPanels.Pop().SetActive(false);
        }

        public void CloseAllOpendedPanels()
        {
            foreach(var panel in openPanels.ToArray())
            {
                panel.SetActive(false);
            }

            openPanels.Clear();
        }

        private void Subscribe()
        {
            _input.PlayerControls.Ui.Back.performed += CloseLastOpenedPanel;
        }

        private void Unsubscribe()
        {
            _input.PlayerControls.Ui.Back.performed -= CloseLastOpenedPanel;
        }

        private void CloseLastOpenedPanel(InputAction.CallbackContext _)
        {
            CloseLastOpenedPanel();
        }
    }
}
