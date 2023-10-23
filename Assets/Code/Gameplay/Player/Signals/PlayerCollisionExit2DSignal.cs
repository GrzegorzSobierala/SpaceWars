using UnityEngine;

namespace Game.Player
{
    public class PlayerCollisionExit2DSignal
    {
        public PlayerCollisionExit2DSignal(Collision2D collision)
        {
            Collision = collision;
        }

        public Collision2D Collision;
    }
}
