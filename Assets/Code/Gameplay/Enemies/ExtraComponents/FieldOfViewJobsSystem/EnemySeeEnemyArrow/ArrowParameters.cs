using System;
using UnityEngine;

namespace Game.Room.Enemy
{
    [Serializable]
    /// <summary>
    /// Represents the parameters for configuring the arrow's behavior and appearance.
    /// </summary>
    public struct ArrowParameters
    {
        public float verdicalDistanceFromTarget;
        public float horizontalDistanceFromTarget;
        public float scale;
        public Vector2 offset;
    }
}
