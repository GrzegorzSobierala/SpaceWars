using System;
using Unity.Mathematics;

namespace Game.Physics
{
    /// <summary>
    /// Represents data for an enemy hit event, including the IDs of the ray-casting enemy and the hit 
    /// enemy collider.
    /// </summary>
    public struct EnemyHitData : IEquatable<EnemyHitData>
    {
        public int rayCasterEnemyId;
        public int hitEnemyColliderId;

        public EnemyHitData (int rayCasterEnemyId, int hitEnemyColliderId)
        {
            this.rayCasterEnemyId = rayCasterEnemyId;
            this.hitEnemyColliderId = hitEnemyColliderId;
        }

        public bool Equals(EnemyHitData other)
        {
            return rayCasterEnemyId == other.rayCasterEnemyId && hitEnemyColliderId == other.hitEnemyColliderId;
        }

        public override int GetHashCode()
        {
            return (int)math.hash(new int2(rayCasterEnemyId, hitEnemyColliderId));
        }
    }
}
