using Game.Input.System;
using Game.Utility;
using UnityEditor.Rendering;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public class PlayerMovement2D : MonoBehaviour
    {
        [Inject] private InputProvider _inputProvider;
        [Inject] private Rigidbody2D _body;

        [SerializeField, Range(0.0f, 60000.0f)] float _moveSpeed = 10000;
        [SerializeField, Range(0.0f, 200.0f)] float _forwardSpeedMulti = 100;
        [SerializeField, Range(0.0f, 200.0f)] float _horizontalSpeedMutli = 50;
        [SerializeField, Range(0.0f, 200.0f)] float _backSpeedMulti = 30;
        [SerializeField] float _rotationSpeed = 50;

        private PlayerControls.GameplayActions _Input => _inputProvider.PlayerControls.Gameplay;

        private Option _lastVerdical = Option.Defult;
        private Option _lastHorizontal = Option.Defult;

        public void VerdicalMove()
        {
            bool moveForward = _Input.MoveForward.ReadValue<float>() == 1.0f;
            bool moveBack = _Input.MoveBack.ReadValue<float>() == 1.0f;

            Option newestSide = LogicUtility.GetNewestOption(moveForward, moveBack, ref _lastVerdical);

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

        public void HorizontalMove()
        {
            bool moveRight = _Input.MoveRight.ReadValue<float>() == 1.0f;
            bool moveLeft = _Input.MoveLeft.ReadValue<float>() == 1.0f;

            Option newestSide = LogicUtility.GetNewestOption(moveRight, moveLeft, ref _lastHorizontal);

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

        public void RotateToCursor()
        {
            Vector2 mousePos = _Input.CursorPosition.ReadValue<Vector2>();
            Vector2 intersectionPoint;
            intersectionPoint = Utils.ScreanPositionOn2DIntersection(mousePos);

            float playerCursorAngle = Utils.AngleDirected(_body.position, intersectionPoint) - 90f;

            float rotSpeed = _rotationSpeed * Time.fixedDeltaTime;
            float targetAngle = Mathf.MoveTowardsAngle(_body.rotation, playerCursorAngle, rotSpeed);

            _body.MoveRotation(targetAngle);
        }
    }
}
