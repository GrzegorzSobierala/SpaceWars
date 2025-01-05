using Game.Input.System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Zenject;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Game.Player.VirtualCamera
{
    public class PlayerFallower : MonoBehaviour
    {
        [Inject] private readonly Rigidbody2D _body;
        [Inject] private readonly CenterOfMass _centerOfMass;
        private Vector3 _offset;

        private void Awake()
        {
            _offset = transform.localPosition;
        }

        private void Update()
        {
            //Vector2 worldCenterOfMassVector = _body.transform.TransformVector(_body.centerOfMass);
            //transform.position = _body.transform.position + (Vector3)worldCenterOfMassVector + _offset;

            transform.position = _centerOfMass.transform.position + _offset;
            //transform.position = (Vector3)_body.position + _offset;
        }


        [Inject] InputProvider input;
        private void LateUpdate()
        {
            Ray ray = Camera.main.ScreenPointToRay(input.PlayerControls.Gameplay.CursorPosition.ReadValue<Vector2>());

            //Debug.Log($"{ray.direction.ToString("f3")} ");
        }
    }
}
