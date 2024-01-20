using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    public class PolygonCollider2DEditor : MonoBehaviour
    {
        public int numberOfPoints = 8;
        public float radius = 1f;

        private void OnValidate()
        {
            UpdateCollider();
        }

        private void UpdateCollider()
        {
            PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();
            if (polygonCollider == null)
            {
                Debug.LogError("PolygonCollider2D component not found.");
                return;
            }

            // Calculate points in a circle
            Vector2[] points = new Vector2[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
            {
                float angle = i * 2 * Mathf.PI / numberOfPoints;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                points[i] = new Vector2(x, y);
            }

            // Set the collider points
            polygonCollider.SetPath(0, points);
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(PolygonCollider2DEditor))]
        public class PolygonCollider2DEditorInspector : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                PolygonCollider2DEditor colliderEditor = (PolygonCollider2DEditor)target;

                if (GUILayout.Button("Update Collider"))
                {
                    colliderEditor.UpdateCollider();
                }
            }
        }
#endif
    }
}
