using UnityEngine;

namespace Game.Physics
{
    public struct FovEntityData
    {
        public Vector2 rayOrigin;
        public float rayDistance;
        public int rayCount;
        public float fovAnlge;
        public float worldAngleAdd;

        public int rayBeforeCount;
        public int vertciesBeforeCount;
    }
}
