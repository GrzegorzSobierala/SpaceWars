using Zenject;
using UnityEngine;

namespace Game.Player
{
    public class OnPlayerCollision2DEnterSignal
    {
        public OnPlayerCollision2DEnterSignal(Collision2D collision)
        {
            Collision = collision;
        }

        public Collision2D Collision;
    }
}
