using UnityEditor;
using UnityEngine;
using Game.Room.Enemy;

namespace Game.Editor
{
    [CustomEditor(typeof(DEPRECATED_EnemyFieldOfView))]
    public class DEPRECATED_EnemyFieldOfViewInspector : SpaceWarsInspector
    {
        private bool isDrawGizmosOn = false;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayoutOption[] options = new GUILayoutOption[0];

            if (GUILayout.Toggle(isDrawGizmosOn, "Draw Gizmos", options))
            {
                isDrawGizmosOn = true;
                DEPRECATED_EnemyFieldOfView enemyFieldOfView = (DEPRECATED_EnemyFieldOfView)target;
                enemyFieldOfView.DrawViewGizmos();
                SceneView.RepaintAll();
            }
            else
            {
                isDrawGizmosOn = false;
            }

            if (GUILayout.Button("Clear and Assign Colliders"))
            {
                DEPRECATED_EnemyFieldOfView enemyFieldOfView = (DEPRECATED_EnemyFieldOfView)target;
                enemyFieldOfView.ClearAndAssignColliders();
            }
        }

        private void OnSceneGUI()
        {
            if (!isDrawGizmosOn)
                return;

            DEPRECATED_EnemyFieldOfView enemyFieldOfView = (DEPRECATED_EnemyFieldOfView)target;
            enemyFieldOfView.DrawViewGizmos();
        }
    }
}
