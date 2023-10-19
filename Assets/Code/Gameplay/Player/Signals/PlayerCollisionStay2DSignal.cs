using UnityEngine;

namespace Game.Player
{
    public class PlayerCollisionStay2DSignal
    {
        public PlayerCollisionStay2DSignal(Collision2D collision)
        {
            Collision = collision;
        }

        public Collision2D Collision;
    }
}
