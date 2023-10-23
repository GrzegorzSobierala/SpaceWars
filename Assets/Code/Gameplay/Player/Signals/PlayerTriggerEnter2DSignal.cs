using UnityEngine;

namespace Game.Player
{
    public class PlayerTriggerEnter2DSignal
    {
        public PlayerTriggerEnter2DSignal(Collider2D collider)
        {
            Collider = collider;
        }

        public Collider2D Collider;
    }
}
