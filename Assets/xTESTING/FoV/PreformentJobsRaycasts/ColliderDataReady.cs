using Unity.Mathematics;
using UnityEngine;

namespace Game.Physics
{
    /// <summary>
    /// Represents prepared collider data ready for raycasting.
    /// </summary>
    public struct ColliderDataReady
    {
        public int type;

        public float2 center;
        public float rotationRad;
        public float2 size;

        public float radius;

        public float2 capsuleA;
        public float2 capsuleB;
        public float capsuleRadius;

        public int vertexStartIndex;
        public int vertexCount;
        public int isClosed;
    }
}
