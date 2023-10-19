using UnityEngine;

namespace Game.Player
{
    public class PlayerTriggerExit2DSignal
    {
        public PlayerTriggerExit2DSignal(Collider2D collider)
        {
            Collider = collider;
        }

        public Collider2D Collider;
    }
}
