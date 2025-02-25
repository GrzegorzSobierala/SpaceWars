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

        private ContactFilter2D _rayFilter;

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

        private void Start()
        {
            Input.Hook.performed += OnHook;
        }

        private void OnDestroy()
        {
            if (GameManager.IsGameQuiting)
                return;

            Input.Hook.performed -= OnHook;
        }

        private void OnHook(InputAction.CallbackContext _)
        {
            if(_hook.IsConnected)
            {
                _hook.Disconnect();
            }
            else
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            Vector2 aimDir;

            if(_gunManager.IsCurrentGunMainGun)
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

            if(isHit == 0)
            {
                return;
            }

            if(!result[0].rigidbody)
            {
                Debug.LogError("No body on collider", result[0].collider);
                return;
            }

            _hook.Connect(result[0].rigidbody, result[0].point);
        }
    }
}
