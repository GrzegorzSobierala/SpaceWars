using UnityEngine;

namespace Game.Physics
{
    /// <summary>
    /// Represents the data required for calculating the field of view (FOV) for an entity.
    /// </summary>
    public struct FovEntityData
    {
        public Vector2 rayOrigin;
        public float rayDistance;
        public int rayCount;
        public float fovAnlge;
        public float worldAngleAdd;

        public int rayBeforeCount;
        public int vertciesBeforeCount;
        public float meshMoveZ;
    }
}
