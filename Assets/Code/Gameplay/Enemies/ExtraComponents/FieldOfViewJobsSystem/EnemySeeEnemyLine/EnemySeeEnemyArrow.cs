using Game.Utility;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class EnemySeeEnemyArrow : MonoBehaviour
    {
        public void SetLine(Vector2 start, Vector2 end, float distanceFromTarget)
        {
            Vector2 backVector = (start - end).normalized * distanceFromTarget;
            transform.position = end + backVector;
            Utils.RotateTowards(transform, end);
        }
    }
}
