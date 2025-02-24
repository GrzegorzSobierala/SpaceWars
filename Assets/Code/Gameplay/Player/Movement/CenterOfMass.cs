using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public class CenterOfMass : MonoBehaviour
    {
        [Inject] private Rigidbody2D _body;

        public Vector2 Position => transform.position;

        private void Start()
        {
            SetInCenterOfMassPosition();
        }

        private void SetInCenterOfMassPosition()
        {
            transform.localPosition = _body.centerOfMass;
        }
    }
}
