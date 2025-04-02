using Game.Utility;
using UnityEngine;

namespace Game.Room.Enemy
{
    public class EnemySeeEnemyLine : MonoBehaviour
    {
        [SerializeField] private float _distanceFromTarget = 35f;

        public void SetLine(Vector2 start, Vector2 end)
        {
            Vector2 backVector = (start - end).normalized * _distanceFromTarget;
            //Vector2 targetPos = (end - start) - backVector;
            transform.position = end + backVector;
            Utils.RotateTowards(transform, end);
        }
    }
}
