using Game.Input.System;
using Game.Player.Control;
using Game.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace Game.Player.Ship
{
    public class PlayerMovement2D : MonoBehaviour
    {
        public Action<int> OnVerdicalMove;
        public Action<int> OnHorizontalMove;
        public Action<Vector2> OnBoost;

        [Inject] private InputProvider _inputProvider;
        [Inject] private Rigidbody2D _body;
        [Inject] private GunManager _gunManager;
        [Inject] private CenterOfMass _centerOfMass;
        [Inject] private CursorCamera _cursorCamera;

        [SerializeField, Range(0.0f, 600.0f)] private float _moveSpeed = 100;
        [SerializeField, Range(0.0f, 200.0f)] private float _forwardSpeedMulti = 100;
        [SerializeField, Range(0.0f, 200.0f)] private float _horizontalSpeedMutli = 50;
        [SerializeField, Range(0.0f, 200.0f)] private float _backSpeedMulti = 30;
        [SerializeField] private float _rotationSpeed = 50;
        [SerializeField] private float _velocityRotTransferMulti = 0.3f;
        [SerializeField] private float _boostCooldown = 2f;
        [SerializeField] private float _boostSpeed = 100;
        [SerializeField] private bool _movementQE = false;
        [SerializeField] private float _oppositeForce = 3;

        private Option _lastVerdical = Option.Default;
        private Option _lastHorizontal = Option.Default;
        
        private float _lastBoostTime = -100;
        private bool _inverseRotWithVerMove = false;

        private List<Collider2D> _lastColldersStucked = new();
        private bool _wasUnstuckCalledThisFrame = false;
        private Vector2 _enginesPower;

        public Vector2 EnginesPower => _enginesPower;

        private PlayerControls.GameplayActions Input => _inputProvider.PlayerControls.Gameplay;

        private InputAction MoveLeft => _inverseRotWithVerMove ? Input.RotateLeft : Input.MoveLeft;
        private InputAction MoveRight => _inverseRotWithVerMove ? Input.RotateRight : Input.MoveRight;
        private InputAction RotateLeft => _inverseRotWithVerMove ? Input.MoveLeft : Input.RotateLeft;
        private InputAction RotateRight => _inverseRotWithVerMove ? Input.MoveRight : Input.RotateRight;

        private void FixedUpdate()
        {
            UpdateMovement();
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            MoveOutOfCollider(collision);
        }

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
                _enginesPower = Utils.ChangeVector2Y(_enginesPower, 1);
                return;
            }
            else if (newestSide == Option.Option2)
            {
                MovePlayer(Vector2.down, _backSpeedMulti);
                OnVerdicalMove?.Invoke(-1);
                _enginesPower = Utils.ChangeVector2Y(_enginesPower, -1);
                return;
            }

            OnVerdicalMove?.Invoke(0);
            _enginesPower = Utils.ChangeVector2Y(_enginesPower, 0);
        }

        public void HorizontalMove()
        {
            bool moveRight;
            bool moveLeft;

            moveRight = MoveRight.ReadValue<float>() == 1.0f;
            moveLeft = MoveLeft.ReadValue<float>() == 1.0f;

            Option newestSide = LogicUtility.GetNewestOption(moveRight, moveLeft, 
                ref _lastHorizontal);

            if (newestSide == Option.Option1)
            {
                MovePlayer(Vector2.right, _horizontalSpeedMutli);
                OnHorizontalMove?.Invoke(1);
                _enginesPower = Utils.ChangeVector2X(_enginesPower, 1);
                return;
            }
            else if (newestSide == Option.Option2)
            {
                MovePlayer(Vector2.left, _horizontalSpeedMutli);
                OnHorizontalMove?.Invoke(-1);
                _enginesPower = Utils.ChangeVector2X(_enginesPower, -1);
                return;
            }

            OnHorizontalMove?.Invoke(0);
            _enginesPower = Utils.ChangeVector2X(_enginesPower, 0);
        }

        public void RotateToCursor()
        {
            if (_movementQE)
                return;

            Vector2 mousePos = Input.CursorPosition.ReadValue<Vector2>();
            RotateToPoint(mousePos);
        }

        public void RotateToPoint(Vector2 point)
        {
            if (_movementQE)
                return;

            Vector2 transCenterOfMass = _centerOfMass.transform.position;

            Vector2 intersectionPoint = _cursorCamera.ScreanPositionOn2DIntersection(point);

            float playerCursorAngle = Utils.AngleDirected(transCenterOfMass, intersectionPoint);

            float rotSpeed = _rotationSpeed * Time.fixedDeltaTime;
            float newAngle = Mathf.MoveTowardsAngle(_body.rotation, playerCursorAngle, rotSpeed);

            _body.MoveRotation(newAngle);
            TransferVelocity(newAngle);
        }

        public void KeyRotate()
        {
            bool rotateLeft = RotateLeft.ReadValue<float>() == 1.0f;
            bool rotateRight = RotateRight.ReadValue<float>() == 1.0f;

            if (rotateLeft && rotateRight)
            {
                RotateByKey(0);
            }
            else if (rotateLeft)
            {
                RotateByKey(1);
            }
            else if (rotateRight)
            {
                RotateByKey(-1);
            }
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

            if (_lastBoostTime + _boostCooldown > Time.time)
                return;

            _lastBoostTime = Time.time;

            TryMovePlayerBoost(boostVector.normalized);
        }

        public void SetVelocityRotMulti(float value)
        {
            _velocityRotTransferMulti = value;
        }

        public void InverseRotWithVerMove()
        {
            _inverseRotWithVerMove = !_inverseRotWithVerMove;
        }

        private void RotateByKey(float value)
        {
            float rotSpeed = (value * _rotationSpeed * Time.fixedDeltaTime) + _body.rotation;
            _body.MoveRotation(rotSpeed);
            TransferVelocity(rotSpeed);
        }

        private void UpdateMovement()
        {
            if(!_gunManager.IsCurrentGunMainGun ||
                RotateLeft.ReadValue<float>() == 1.0f || 
                RotateRight.ReadValue<float>() == 1.0f)
            {
                KeyRotate();
            }
            else
            {
                RotateToCursor();
            }

            VerdicalMove();
            HorizontalMove();
            TryBoost();

            if(_wasUnstuckCalledThisFrame)
            {
                _wasUnstuckCalledThisFrame = false;
            }    
            else
            {
                _lastColldersStucked.Clear();
            }
        }

        private void MovePlayer(Vector2 direction, float procentOfMaxSpeed)
        {
            Vector2 worldDirection = Utils.LocalToWorldDirection(direction, _body.transform);

            float dot = Vector2.Dot(worldDirection.normalized, _body.velocity.normalized);
            float oppositeSideMulti = 1;
            if(dot < 0)
            {
                oppositeSideMulti += -dot * _oppositeForce;
            }

            Vector2 targetForce = _body.mass * _moveSpeed * oppositeSideMulti * direction;
            _body.AddRelativeForce(procentOfMaxSpeed * Time.fixedDeltaTime * targetForce);
        }
        
        private void TransferVelocity(float angle)
        {
            float relativeAngle = Mathf.DeltaAngle(_body.rotation, angle);
            float velocityAngle = relativeAngle * _velocityRotTransferMulti;
            _body.velocity = Utils.RotateVector(_body.velocity, velocityAngle);
        }

        private void TryMovePlayerBoost(Vector2 direction)
        {
            Vector2 targetForce = _body.mass * _boostSpeed * direction;
            Vector2 targetForceRel = _body.GetRelativeVector(targetForce);

            float dotProduct = Vector2.Dot(_body.velocity.normalized, targetForceRel.normalized);

            if (dotProduct <= Utils.AngleToDotProduct(67.5f))
            {
                _body.velocity = Vector2.zero;
            }

            _body.AddRelativeForce(targetForce);
            OnBoost?.Invoke(direction);
        }

        private void MoveOutOfCollider(Collision2D collision)
        {
            Collider2D collider = collision.collider;

            if (!collider)
            {
                return;
            }

            Rigidbody2D colliderBody = collider.attachedRigidbody;
            if (colliderBody && colliderBody.bodyType == RigidbodyType2D.Dynamic)
            {
                return;
            }

            if (!collider.OverlapPoint(_body.position))
            {
                return;
            }

            Vector2 collderToPlayerDir = (_body.position - (Vector2)collider.bounds.center);
            collderToPlayerDir = collderToPlayerDir.normalized;
            float moveDistance = collider.bounds.extents.magnitude;
            float unStackForce = 0.5f + (_lastColldersStucked.Count * 0.1f);
            moveDistance *= unStackForce;
            Vector2 targetPos = _body.position + collderToPlayerDir * moveDistance;

            if(_lastColldersStucked.Count < 2)
            {
                _body.MovePosition(targetPos);
                Debug.Log($"Unstucking player force {unStackForce}", this);
            }
            else
            {
                _body.position = targetPos;
                Debug.Log($"HARD unstucking player force {unStackForce}", this);
            }
            _lastColldersStucked.Add(collider);
            _wasUnstuckCalledThisFrame = true;
        }
    }
}
