using UnityEngine;
using Zenject;

namespace Game
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
            transform.position = _body.transform.position + _offset;
        }
    }
}
