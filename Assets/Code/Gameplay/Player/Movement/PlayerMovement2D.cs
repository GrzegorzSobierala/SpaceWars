using Game.Input.System;
using Game.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game
{
    public class PlayerMovement2D : MonoBehaviour
    {
        [Inject] InputProvider _inputProvider;

        [SerializeField] private Rigidbody2D _body;

        [SerializeField, Range(0.0f, 60000.0f)] float _moveSpeed = 10000;
        [SerializeField, Range(0.0f, 200.0f)] float _horizontalSpeedMutli = 50;
        [SerializeField, Range(0.0f, 200.0f)] float _backSpeedMulti = 30;
        [SerializeField, Range(0.0f, 200.0f)] float _forwardSpeedMulti = 100;
        [SerializeField] float _rotationSpeed = 50;

        private PlayerControls.GameplayActions Input => _inputProvider.PlayerControls.Gameplay; 

        private void FixedUpdate()
        {
            VerdicalMove();
        }

        private void Start()
        {
            _inputProvider.PlayerControls.Gameplay.Enable();
        }

        private void VerdicalMove()
        {
            if (Input.MoveForward.ReadValue<float>() == 1.0f)
            {
                MovePlayer(Vector2.up, _forwardSpeedMulti);
                return;
            }
            else if (Input.MoveBack.ReadValue<float>() == 1.0f)
            {
                MovePlayer(Vector2.down, _backSpeedMulti);
                return;
            }
            else if (Input.MoveLeft.ReadValue<float>() == 1.0f)
            {
                MovePlayer(Vector2.left, _horizontalSpeedMutli);
                return;
            }
            else if (Input.MoveRight.ReadValue<float>() == 1.0f)
            {
                MovePlayer(Vector2.right, _horizontalSpeedMutli);
                return;
            }
        }

        private void MovePlayer(Vector2 direction, float procentOfMaxSpeed)
        {
            _body.AddRelativeForce(direction * _moveSpeed * (procentOfMaxSpeed) * Time.fixedDeltaTime);
        }

        //to test
        private void RotateToCursor()
        {
            Vector2 mousePos = Input.LookDirection.ReadValue<Vector2>();
            Vector2 playerPos = transform.position;
            Vector2 direction = mousePos - playerPos;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, _rotationSpeed * Time.deltaTime);
        }
    }
}
