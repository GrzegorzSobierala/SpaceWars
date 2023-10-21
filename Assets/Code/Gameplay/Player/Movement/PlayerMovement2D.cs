using Game.Input.System;
using Game.Utility;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public class PlayerMovement2D : MonoBehaviour
    {
        [Inject] InputProvider _inputProvider;
        [Inject] private Rigidbody2D _body;

        [SerializeField, Range(0.0f, 60000.0f)] float _moveSpeed = 10000;
        [SerializeField, Range(0.0f, 200.0f)] float _forwardSpeedMulti = 100;
        [SerializeField, Range(0.0f, 200.0f)] float _horizontalSpeedMutli = 50;
        [SerializeField, Range(0.0f, 200.0f)] float _backSpeedMulti = 30;
        [SerializeField] float _rotationSpeed = 50;

        private PlayerControls.GameplayActions _Input => _inputProvider.PlayerControls.Gameplay;

        private Option _lastUsedOption = Option.Defult;

        private void FixedUpdate()
        {
            RotateToCursor();
            HorizontalMove();
            VerdicalMove();
        }

        private void VerdicalMove()
        {
            bool moveForward = _Input.MoveForward.ReadValue<float>() == 1.0f;
            bool moveBack = _Input.MoveBack.ReadValue<float>() == 1.0f;

            Option newestSide = LogicUtility.GetNewestOption(moveForward, moveBack, ref _lastUsedOption);

            if (newestSide == Option.Option1)
            {
                MovePlayer(Vector2.up, _forwardSpeedMulti);
                return;
            }
            else if (newestSide == Option.Option2)
            {
                MovePlayer(Vector2.down, _backSpeedMulti);
                return;
            }
        }

        private void HorizontalMove()
        {
            bool moveRight = _Input.MoveRight.ReadValue<float>() == 1.0f;
            bool moveLeft = _Input.MoveLeft.ReadValue<float>() == 1.0f;

            Option newestSide = LogicUtility.GetNewestOption(moveRight, moveLeft, ref _lastUsedOption);

            if (newestSide == Option.Option1)
            {
                MovePlayer(Vector2.right, _horizontalSpeedMutli);
                return;
            }
            else if (newestSide == Option.Option2)
            {
                MovePlayer(Vector2.left, _horizontalSpeedMutli);
                return;
            }
        }

        private void MovePlayer(Vector2 direction, float procentOfMaxSpeed)
        {
            _body.AddRelativeForce(procentOfMaxSpeed * _moveSpeed * Time.fixedDeltaTime * direction);
        }

        private void RotateToCursor()
        {
            Vector2 cursorPos = _Input.CursorPosition.ReadValue<Vector2>();
            Vector2 playerPos = TransformUtility.WorldToScreenPointClamped(_body.position);

            Vector2 playerCursorDirection = cursorPos - playerPos;

            float playerCursorAngle = Mathf.Atan2(playerCursorDirection.y, playerCursorDirection.x);
            playerCursorAngle = (playerCursorAngle - Mathf.PI / 2) * Mathf.Rad2Deg;

            if (playerCursorAngle > 180f)
            {
                playerCursorAngle -= 360f;
            }
            else if (playerCursorAngle < -180f)
            {
                playerCursorAngle += 360f;
            }

            float rotSpeed = _rotationSpeed * Time.fixedDeltaTime;
            float targetAngle = Mathf.MoveTowardsAngle(_body.rotation, playerCursorAngle, rotSpeed);

            _body.MoveRotation(targetAngle);
        }
    }
}
