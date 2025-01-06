using UnityEngine;
using Zenject;

namespace Game.Player.Ship
{
    public class CenterOfMass : MonoBehaviour
    {
        [Inject] private Rigidbody2D _body;

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
