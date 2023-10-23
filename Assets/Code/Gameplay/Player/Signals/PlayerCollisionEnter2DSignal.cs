using UnityEngine;

namespace Game.Player
{
    public class PlayerCollisionEnter2DSignal
    {
        public PlayerCollisionEnter2DSignal(Collision2D collision)
        {
            Collision = collision;
        }

        public Collision2D Collision;
    }
}
