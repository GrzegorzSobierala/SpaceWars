using Game.Input.System;
using Game.Utility;
using System;
using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public class PlayerMovement2D : MonoBehaviour
    {
        [Inject] private InputProvider _inputProvider;
        [Inject] private Rigidbody2D _body;

        [SerializeField, Range(0.0f, 600.0f)] private float _moveSpeed = 100;
        [SerializeField, Range(0.0f, 200.0f)] private float _forwardSpeedMulti = 100;
        [SerializeField, Range(0.0f, 200.0f)] private float _horizontalSpeedMutli = 50;
        [SerializeField, Range(0.0f, 200.0f)] private float _backSpeedMulti = 30;
        [SerializeField] private float _rotationSpeed = 50;
        [SerializeField] private float _velocityRotMulti = 0.5f;
        [SerializeField] private float _boostCooldown = 2f;
        [SerializeField] private float _boostSpeed = 100;

        private PlayerControls.GameplayActions Input => _inputProvider.PlayerControls.Gameplay;

        private Option _lastVerdical = Option.Default;
        private Option _lastHorizontal = Option.Default;

        public Action<int> OnVerdicalMove;
        public Action<int> OnHorizontalMove;

        private float lastBoostTime = -100;

        public void VerdicalMove()
        {
            bool moveForward = Input.MoveForward.ReadValue<float>() == 1.0f;
            bool moveBack = Input.MoveBack.ReadValue<float>() == 1.0f;

            Option newestSide = LogicUtility.GetNewestOption(moveForward, moveBack, 
                ref _lastVerdical);

            if (newestSide == Option.Option1)
            {
                MovePlayer(Vector2.up, _forwardSpeedMulti);
                OnVerdicalMove?.Invoke(1);
                return;
            }
            else if (newestSide == Option.Option2)
            {
                MovePlayer(Vector2.down, _backSpeedMulti);
                OnVerdicalMove?.Invoke(-1);
                return;
            }

            OnVerdicalMove?.Invoke(0);
        }

        public void HorizontalMove()
        {
            bool moveRight = Input.MoveRight.ReadValue<float>() == 1.0f;
            bool moveLeft = Input.MoveLeft.ReadValue<float>() == 1.0f;

            Option newestSide = LogicUtility.GetNewestOption(moveRight, moveLeft, 
                ref _lastHorizontal);

            if (newestSide == Option.Option1)
            {
                MovePlayer(Vector2.right, _horizontalSpeedMutli);
                OnHorizontalMove?.Invoke(1);
                return;
            }
            else if (newestSide == Option.Option2)
            {
                MovePlayer(Vector2.left, _horizontalSpeedMutli);
                OnHorizontalMove?.Invoke(-1);
                return;
            }
            OnHorizontalMove?.Invoke(0);
        }

        public void TryBoost()
        {
            if (Input.Boost.ReadValue<float>() != 1.0f)
                return;

            Vector2 boostVector = Vector2.zero;

            if(_lastVerdical == Option.Option1)
            {
                boostVector += Vector2.up;
            }
            else if (_lastVerdical == Option.Option2)
            {
                boostVector += Vector2.down;
            }

            if(_lastHorizontal == Option.Option1)
            {
                boostVector += Vector2.right;
            }
            else if (_lastHorizontal == Option.Option2)
            {
                boostVector += Vector2.left;
            }

            if (boostVector == Vector2.zero)
                return;

            if (lastBoostTime + _boostCooldown > Time.time)
                return;

            lastBoostTime = Time.time;

            TryMovePlayerBoost(boostVector.normalized);
        }

        private void MovePlayer(Vector2 direction, float procentOfMaxSpeed)
        {
            Vector2 targetForce = direction * _moveSpeed * _body.mass;
            _body.AddRelativeForce(procentOfMaxSpeed * targetForce * Time.fixedDeltaTime);
        }

        public void RotateToCursor()
        {
            Vector2 mousePos = Input.CursorPosition.ReadValue<Vector2>();
            RotateToPoint(mousePos);
        }

        public void RotateToPoint(Vector2 point)
        {
            Vector2 intersectionPoint = Utils.ScreanPositionOn2DIntersection(point);
            float playerCursorAngle = Utils.AngleDirected(_body.position, intersectionPoint) - 90f;

            float rotSpeed = _rotationSpeed * Time.fixedDeltaTime;
            float newAngle = Mathf.MoveTowardsAngle(_body.rotation, playerCursorAngle, rotSpeed);

            _body.MoveRotation(newAngle);

            TransferVelocity(newAngle);
        }

        private void TransferVelocity(float angle)
        {
            // Calculate the relative angle between current rotation and target rotation
            float relativeAngle = Mathf.DeltaAngle(_body.rotation, angle);

            // Calculate the angular velocity based on the relative angle and _velocityRotMulti
            float velocityAngle = relativeAngle * _velocityRotMulti;

            // Rotate the current velocity vector
            _body.velocity = Utils.RotateVector(_body.velocity, velocityAngle);
        }

        public void SetVelocityRotMulti(float value)
        {
            _velocityRotMulti = value;
        }

        private void TryMovePlayerBoost(Vector2 direction)
        {
            Vector2 targetForce = direction * _boostSpeed * _body.mass;
            Vector2 targetForceRel = _body.GetRelativeVector(targetForce);

            float dotProduct = Vector2.Dot(_body.velocity.normalized, targetForceRel.normalized);

            if (dotProduct <= Utils.AngleToDotProduct(67.5f))
            {
                _body.velocity = Vector2.zero;
            }

            _body.AddRelativeForce(targetForce);
        }
    }
}
