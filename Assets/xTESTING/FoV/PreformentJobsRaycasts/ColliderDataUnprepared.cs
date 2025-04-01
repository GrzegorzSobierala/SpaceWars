using UnityEngine;

namespace Game.Physics
{
    /// <summary>
    /// Represents unprepared collider data before it is processed for raycasting.
    /// </summary>
    public struct ColliderDataUnprepared
    {
        public ColliderType typeEnum;
        public Vector3 posWorld;
        public Vector2 offsetLoc;
        public float rotWorld;
        public Vector3 lossyScale;
        public Vector2 sizeLoc;

        public float radiusLoc;

        public CapsuleDirection2D capsuleDirEnum;
        public Vector3 capsuleTransUpOrBoundsPos;

        public Vector3 capsuleTransRightOrBoundsSize;
        public int vertexStartIndex;
        public int vertexCount;

        public int colliderId;
        public int layer;
    }
}
