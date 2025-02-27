using Game.Input.System;
using Game.Management;
using Game.Player.Control;
using Game.Utility;
using Game.Utility.Globals;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Game.Player.Ship
{
    public class HookCannon : MonoBehaviour
    {
        [Inject] private InputProvider _inputProvider;
        [Inject] private Hook _hook;
        [Inject] private CenterOfMass _centerOfMass;
        [Inject] private CursorCamera _cursorCamera;
        [Inject] private GunManager _gunManager;
        [Inject] private Rigidbody2D _body;
        [Inject] private PlayerMovement2D _playerMovement;

        [SerializeField] private Transform _hookTargetVisual;

        private ContactFilter2D _rayFilter;
        private ConnectState _connectState = ConnectState.WaitForConnectInput;

        private enum ConnectState
        {
            WaitForConnectInput = 0,
            WaitForConnect = 1,
            WaitForCancelConnectInput = 2,
            WaitForDisconnectInput = 3,
            WaitForCancelDisconnectInput = 4
        }

        private PlayerControls.GameplayActions Input => _inputProvider.PlayerControls.Gameplay;

        private void Awake()
        {
            _rayFilter = new ContactFilter2D
            {
                useTriggers = false,
                useLayerMask = true,
                layerMask = LayerMask.GetMask(Layers.Enemy, Layers.Obstacle, Layers.ObstacleShootAbove,
                Layers.SmallObstacle)
            };
        }

        private void Update()
        {
            UpdateCannon();
            UpdateHookTargetVisual();
        }

        private void UpdateCannon()
        {
            bool isPressed = Input.Hook.ReadValue<float>() >= 0.5f;

            switch (_connectState)
            {
                case ConnectState.WaitForConnectInput:
                    OnWaitForConnectInput(isPressed);
                    break;
                case ConnectState.WaitForConnect:
                    OnWaitForConnect(isPressed);
                    break;
                case ConnectState.WaitForCancelConnectInput:
                    OnWaitForCancelConnectInput(isPressed);
                    break;
                case ConnectState.WaitForDisconnectInput:
                    OnWaitForDisconnectInput(isPressed);
                    break;
                case ConnectState.WaitForCancelDisconnectInput:
                    OnWaitForCancelDisconnectInput(isPressed);
                    break;
                default:
                    Debug.LogError("Unknown cannon state", this);
                    break;
            }
        }

        private void OnWaitForConnectInput(bool isPressed)
        {
            if(isPressed)
            {
                _connectState = ConnectState.WaitForConnect;
                OnWaitForConnect(isPressed);
            }
        }

        private void OnWaitForConnect(bool isPressed)
        {
            if (isPressed)
            {
                TryConnect();
                if (_hook.IsConnected)
                {
                    _connectState = ConnectState.WaitForCancelConnectInput;
                }
            }
            else
            {
                _connectState = ConnectState.WaitForConnectInput;
            }
        }

        private void OnWaitForCancelConnectInput(bool isPressed)
        {
            if (!_hook.IsConnected)
            {
                _connectState = ConnectState.WaitForCancelDisconnectInput;
            }
            else if (!isPressed)
            {
                _connectState = ConnectState.WaitForDisconnectInput;
            }
        }

        private void OnWaitForDisconnectInput(bool isPressed)
        {
            if (!_hook.IsConnected)
            {
                _connectState = ConnectState.WaitForConnectInput;
            }
            else if (isPressed)
            {
                _hook.Disconnect();
                _connectState = ConnectState.WaitForCancelDisconnectInput;
            }
        }

        private void OnWaitForCancelDisconnectInput(bool isPressed)
        {
            if (!isPressed)
            {
                _connectState = ConnectState.WaitForConnectInput;
            }
        }

        private void TryConnect()
        {
            RaycastHit2D? hit = ConnectRaycast();

            if(hit == null)
                return;

            if (!hit.Value.rigidbody)
            {
                Rigidbody2D addedBody = hit.Value.collider.gameObject.AddComponent<Rigidbody2D>();
                addedBody.bodyType = RigidbodyType2D.Static;
                Debug.LogError("No body on collider, added statid body", hit.Value.collider);
                _hook.Connect(addedBody, hit.Value.point);
                return;
            }

            _hook.Connect(hit.Value.rigidbody, hit.Value.point);
        }

        private void UpdateHookTargetVisual()
        {
            RaycastHit2D? hit = ConnectRaycast();

            if (hit == null)
            {
                _hookTargetVisual.gameObject.SetActive(false);
                return;
            }

            _hookTargetVisual.gameObject.SetActive(true);
            _hookTargetVisual.position = hit.Value.point;
        }

        private RaycastHit2D? ConnectRaycast()
        {
            Vector2 aimDir;

            if (_gunManager.IsCurrentGunMainGun)
            {
                Vector2 aimDirLocal;
                if (_playerMovement.EnginesPower.x == 0)
                {
                    aimDirLocal = Vector2.up;
                }
                else
                {
                    aimDirLocal = Utils.ChangeVector2Y(_playerMovement.EnginesPower, 0);
                }

                aimDir = _body.transform.TransformDirection(aimDirLocal).normalized;
            }
            else
            {
                Vector2 mousePos = Input.CursorPosition.ReadValue<Vector2>();
                Vector2 aimPoint = _cursorCamera.ScreanPositionOn2DIntersection(mousePos);
                aimDir = (aimPoint - _centerOfMass.Position).normalized;
            }

            RaycastHit2D[] result = new RaycastHit2D[1];
            int isHit = Physics2D.Raycast(_centerOfMass.Position, aimDir, _rayFilter, result,
                _hook.MaxDistance);

            if (isHit == 0)
            {
                return null;
            }

            return result[0];        }
    }
}
