using UnityEngine;

namespace Game.Player
{
    public class PlayerCollisionStay2DSignal : MonoBehaviour
    {
        public PlayerCollisionStay2DSignal(Collision2D collision)
        {
            Collision = collision;
        }

        public Collision2D Collision;
    }
}
