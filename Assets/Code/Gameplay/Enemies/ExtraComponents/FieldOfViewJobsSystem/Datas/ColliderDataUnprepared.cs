using Unity.Mathematics;
using UnityEngine;

namespace Game.Physics
{
    /// <summary>
    /// Represents data copied from type fo UnityEngine.Collider2D to be used in PrepareColliderDatasJob
    /// to make use of BurstCompiler and prepare the data for raycast job.
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
