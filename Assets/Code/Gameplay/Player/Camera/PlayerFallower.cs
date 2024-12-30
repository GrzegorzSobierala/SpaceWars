using UnityEngine;
using Zenject;

namespace Game.Player.VirtualCamera
{
    public class PlayerFallower : MonoBehaviour
    {
        [Inject] private readonly Rigidbody2D _body;
        private Vector3 _offset;

        private void Awake()
        {
            _offset = transform.localPosition;
        }

        private void Update()
        {
            Vector2 worldCenterOfMassVector = _body.transform.TransformVector(_body.centerOfMass);
            transform.position = _body.transform.position + (Vector3)worldCenterOfMassVector + _offset;
        }
    }
}
