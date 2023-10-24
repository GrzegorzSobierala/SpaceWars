using UnityEngine;

namespace Game.Utility
{
    public static class TransformUtility
    {
        public static Vector3 WorldToScreenPointClamped(Vector3 position)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(position);

            screenPosition.x = Mathf.Clamp(screenPosition.x, 0f, Screen.width);
            screenPosition.y = Mathf.Clamp(screenPosition.y, 0f, Screen.height);

            return screenPosition;
        }

        /// <summary>
        /// Screan point to 2D player plane intersection point 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector2 ScreanToPlayerIntersection(Vector2 position) 
        {
            Ray ray = Camera.main.ScreenPointToRay(position);
            float t = -Camera.main.transform.position.z / ray.direction.z;
            return ray.origin + t * ray.direction;
        }
    }
}
