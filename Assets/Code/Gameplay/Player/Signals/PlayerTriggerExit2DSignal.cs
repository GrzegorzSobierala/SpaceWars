using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PlayerTriggerExit2DSignal : MonoBehaviour
    {
        public PlayerTriggerExit2DSignal(Collider2D collider)
        {
            Collider = collider;
        }

        public Collider2D Collider;
    }
}
