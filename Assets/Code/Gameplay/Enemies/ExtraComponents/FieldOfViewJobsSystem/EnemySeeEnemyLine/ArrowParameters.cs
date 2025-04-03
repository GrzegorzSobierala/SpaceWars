using System;
using UnityEngine;

namespace Game.Room.Enemy
{
    [Serializable]
    public struct ArrowParameters
    {
        public float verdicalDistanceFromTarget;
        public float horizontalDistanceFromTarget;
        public float scale;
        public Vector2 offset;
    }
}
