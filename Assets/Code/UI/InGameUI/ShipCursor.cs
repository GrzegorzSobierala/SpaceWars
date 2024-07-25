using Game.Input.System;
using Game.Management;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace Game.Player.UI
{
    public class ShipCursor : MonoBehaviour
    {
        [Inject] private InputProvider _inputProvider;
        [Inject] private PlayerManager _playerManager;

        [SerializeField] private GameObject _imagesParent;
        [SerializeField] private Image _leftImage;
        [SerializeField] private Image _rightImage;
        [Space]
        [SerializeField] private Color _leftImageActiveColor;
        [SerializeField] private Color _leftImageInactiveColor;
        [SerializeField] private Color _rightImageActiveColor;
        [SerializeField] private Color _rightImageInactiveColor;
        [Space]
        [SerializeField] private UnityEvent _onLeftImageActive;
        [SerializeField] private UnityEvent _onLeftImageInactive;
        [SerializeField] private UnityEvent _onRightImageActive;
        [SerializeField] private UnityEvent _onRightImageInactive;

        private Vector3 _startLeftImageSize;
        private Vector3 _startRightImageSize;

        private PlayerControls.GameplayActions Input => _inputProvider.PlayerControls.Gameplay;

        private void Start()
        {
            Init();
            SubscribeInput();
        }

        private void OnDestroy()
        {
            UnsubscribeInput();
        }

        private void Update()
        {
            UpdateCursor();
        }

        #region EventMethods

        public void SetLeftImageScale(float scale)
        {
            _leftImage.transform.localScale = _startRightImageSize * scale;
        }

        public void SetLeftImageColorActive()
        {
            _leftImage.color = _leftImageActiveColor;
        }

        public void SetLeftImageColorInactive()
        {
            _leftImage.color = _leftImageInactiveColor;
        }

        public void SetRightImageScale(float scale)
        {
            _rightImage.transform.localScale = _startLeftImageSize * scale;
        }

        public void SetRightImageColorActive()
        {
            _rightImage.color = _rightImageActiveColor;
        }

        public void SetRightImageColorInactive()
        {
            _rightImage.color = _rightImageInactiveColor;
        }

        #endregion

        private void Init()
        {
            _imagesParent.SetActive(true);
            _startLeftImageSize = _leftImage.transform.localScale;
            _startRightImageSize = _rightImage.transform.localScale;
        }

        private void UpdateCursor()
        {
            Vector3 vectorTargetRot = new Vector3(0, 0, _playerManager.PlayerBody.rotation);
            Quaternion targetRot = Quaternion.Euler(vectorTargetRot);
            transform.rotation = targetRot;
            transform.position = Input.CursorPosition.ReadValue<Vector2>();
        }

        private void SubscribeInput()
        {
            Input.MoveLeft.performed += SetLeftImageActive;
            Input.MoveLeft.canceled += SetLeftImageInactive;
            Input.MoveRight.performed += SetRightImageActive;
            Input.MoveRight.canceled += SetRightImageInactive;
            Input.RotateLeft.performed += SetLeftImageActive;
            Input.RotateLeft.canceled += SetLeftImageInactive;
            Input.RotateRight.performed += SetRightImageActive;
            Input.RotateRight.canceled += SetRightImageInactive;
        }

        private void UnsubscribeInput()
        {
            Input.MoveLeft.performed -= SetLeftImageActive;
            Input.MoveLeft.canceled -= SetLeftImageInactive;
            Input.MoveRight.performed -= SetRightImageActive;
            Input.MoveRight.canceled -= SetRightImageInactive;
            Input.RotateLeft.performed -= SetLeftImageActive;
            Input.RotateLeft.canceled -= SetLeftImageInactive;
            Input.RotateRight.performed -= SetRightImageActive;
            Input.RotateRight.canceled -= SetRightImageInactive;
        }

        private void SetLeftImageActive(InputAction.CallbackContext context)
        {
            _onLeftImageActive?.Invoke();
        }

        private void SetLeftImageInactive(InputAction.CallbackContext context)
        {
            _onLeftImageInactive?.Invoke();
        }

        private void SetRightImageActive(InputAction.CallbackContext context)
        {
            _onRightImageActive?.Invoke();
        }

        private void SetRightImageInactive(InputAction.CallbackContext context)
        {
            _onRightImageInactive?.Invoke();
        }
    }
}
