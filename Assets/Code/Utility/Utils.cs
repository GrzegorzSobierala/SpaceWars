using UnityEngine;

namespace Game.Utility
{
    public static class Utils
    {
        public static Vector3 WorldToScreenPointClamped(Vector3 position)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(position);

            screenPosition.x = Mathf.Clamp(screenPosition.x, 0f, Screen.width);
            screenPosition.y = Mathf.Clamp(screenPosition.y, 0f, Screen.height);

            return screenPosition;
        }

        /// <summary>
        /// Screan point to 2D phisic plane intersection point 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector2 ScreanPositionOn2DIntersection(Vector2 position) 
        {
            Ray ray = Camera.main.ScreenPointToRay(position);
            float t = -Camera.main.transform.position.z / ray.direction.z;
            return ray.origin + t * ray.direction;
        }

        /// <summary>
        /// Get angle from vector(1,0), return value from left is negative, value on the right is positive
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static float AngleDirected(Vector2 vector)
        {
            float angle = Mathf.Atan2(vector.y, vector.x);
            return angle * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Get angle from vector(1,0), return value from left is negative, value on the right is positive
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static float AngleDirected(Vector2 startVectorPos , Vector2 endVectorPos)
        {
            return AngleDirected(endVectorPos - startVectorPos);
        }
    }
}
