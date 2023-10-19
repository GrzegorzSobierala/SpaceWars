using UnityEngine;

namespace Game.Player
{
    public class PlayerTriggerStay2DSignal
    {
        public PlayerTriggerStay2DSignal(Collider2D collider)
        {
            Collider = collider;
        }

        public Collider2D Collider;
    }
}
