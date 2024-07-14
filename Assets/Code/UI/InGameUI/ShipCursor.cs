using Game.Input.System;
using Game.Management;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Player.UI
{
    public class ShipCursor : MonoBehaviour
    {
        [Inject] private InputProvider _inputProvider;
        [Inject] private PlayerManager _playerManager;

        [SerializeField] private GameObject _imagesParent;

        private PlayerControls.GameplayActions Input => _inputProvider.PlayerControls.Gameplay;

        private void Start()
        {
            _imagesParent.SetActive(true);
        }

        private void Update()
        {
            Vector3 vectorTargetRot = new Vector3(0, 0, _playerManager.PlayerBody.rotation);
            Quaternion targetRot = Quaternion.Euler(vectorTargetRot);
            transform.rotation = targetRot;
            transform.position = Input.CursorPosition.ReadValue<Vector2>();
        }
    }
}
