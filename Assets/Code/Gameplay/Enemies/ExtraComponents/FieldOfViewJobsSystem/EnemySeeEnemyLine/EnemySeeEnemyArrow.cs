using Game.Utility;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class EnemySeeEnemyArrow : MonoBehaviour
    {
        public void SetLine(Vector2 start, Vector2 end, float distanceFromTarget)
        {
            Vector2 backVector = (start - end).normalized * distanceFromTarget;
            transform.position = (Vector3)(end + backVector) + new Vector3(0,0, -0.05f);
            Utils.RotateTowards(transform, end);
        }
    }
}
