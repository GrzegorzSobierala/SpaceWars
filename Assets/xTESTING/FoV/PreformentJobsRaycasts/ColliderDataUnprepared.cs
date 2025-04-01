using Unity.Mathematics;
using UnityEngine;

namespace Game.Physics
{
    /// <summary>
    /// Represents unprepared collider data before it is processed for raycasting.
    /// </summary>
    public struct ColliderDataUnprepared
    {
        public ColliderType typeEnum;
        public float2 posWorld;
        public float2 offsetLoc;
        public float rotWorld;
        public float2 lossyScale;
        public float2 sizeLoc;

        public float radiusLoc;

        public CapsuleDirection2D capsuleDirEnum;
        public float2 capsuleTransUpOrBoundsPos;

        public float2 capsuleTransRightOrBoundsSize;
        public int vertexStartIndex;
        public int vertexCount;

        public int colliderId;
        public int layer;
    }
}
