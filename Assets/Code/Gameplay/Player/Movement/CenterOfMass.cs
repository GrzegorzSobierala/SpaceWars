using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game
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
