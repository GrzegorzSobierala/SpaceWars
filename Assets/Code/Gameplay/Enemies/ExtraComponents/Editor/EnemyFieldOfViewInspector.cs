using UnityEditor;
using UnityEngine;
using Game.Room.Enemy;

namespace Game.Editor
{
    [CustomEditor(typeof(EnemyFieldOfView))]
    public class EnemyFieldOfViewInspector : SpaceWarsInspector
    {
        private bool isDrawGizmosOn = false;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayoutOption[] options = new GUILayoutOption[0];

            if (GUILayout.Toggle(isDrawGizmosOn, "Draw Gizmos", options))
            {
                isDrawGizmosOn = true;
                EnemyFieldOfView enemyFieldOfView = (EnemyFieldOfView)target;
                enemyFieldOfView.DrawViewGizmos();
                SceneView.RepaintAll();
            }
            else
            {
                isDrawGizmosOn = false;
            }

            if (GUILayout.Button("Clear and Assign Colliders"))
            {
                EnemyFieldOfView enemyFieldOfView = (EnemyFieldOfView)target;
                enemyFieldOfView.ClearAndAssignColliders();
            }
        }

        private void OnSceneGUI()
        {
            if (!isDrawGizmosOn)
                return;

            EnemyFieldOfView enemyFieldOfView = (EnemyFieldOfView)target;
            enemyFieldOfView.DrawViewGizmos();
        }
    }
}
