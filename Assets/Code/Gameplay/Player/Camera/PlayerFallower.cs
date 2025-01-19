using Game.Player.Ship;
using NaughtyAttributes;
using UnityEngine;
using Zenject;

namespace Game.Player.Control
{
    public class PlayerFallower : MonoBehaviour
    {
        [Inject] private readonly CenterOfMass _centerOfMass;

        [SerializeField] private Vector3 _offset;

        private void Awake()
        {
            Move();
        }

        private void Update()
        {
            Move();
        }

        private void Move()
        {
            transform.position = _centerOfMass.transform.position + _offset;
        }

        [Button]
        protected void SetOffsetFromLocalPos()
        {
            _offset = transform.localPosition;
        }
    }
}
