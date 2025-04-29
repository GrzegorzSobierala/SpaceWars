using Unity.Mathematics;

namespace Game.Physics
{
    /// <summary>
    /// Represents prepared collider data ready for raycast job.
    /// </summary>
    public struct ColliderDataReady
    {
        public ColliderType type;

        public float2 center;
        public float rotationRad;
        public float2 size;

        public float radius;

        public float2 capsuleAOrBoundsPos;
        public float2 capsuleBOrBoundsSize;
        public float capsuleRadius;

        public int vertexStartIndex;
        public int vertexCount;
        public int isClosed;

        public int colliderId;
        public int layer;
    }
}
