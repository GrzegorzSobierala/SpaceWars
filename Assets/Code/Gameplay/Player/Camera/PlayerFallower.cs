using Game.Player.Ship;
using UnityEngine;
using Zenject;

namespace Game.Player.Control
{
    public class PlayerFallower : MonoBehaviour
    {
        [Inject] private readonly CenterOfMass _centerOfMass;
        private Vector3 _offset;

        private void Awake()
        {
            _offset = transform.localPosition;
        }

        private void Update()
        {
            transform.position = _centerOfMass.transform.position + _offset;
        }
    }
}
