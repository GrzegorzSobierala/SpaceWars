using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PlayerTriggerStay2DSignal : MonoBehaviour
    {
        public PlayerTriggerStay2DSignal(Collider2D collider)
        {
            Collider = collider;
        }

        public Collider2D Collider;
    }
}
