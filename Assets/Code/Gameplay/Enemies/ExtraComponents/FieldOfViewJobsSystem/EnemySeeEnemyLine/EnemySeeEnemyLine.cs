using UnityEngine;

namespace Game.Room.Enemy
{
    public class EnemySeeEnemyLine : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Transform _image;

        public void SetLine(Vector2 start, Vector2 end)
        {
            _lineRenderer.SetPosition(0, start);
            _lineRenderer.SetPosition(1, end);
            //_image.position = end;
        }
    }
}
