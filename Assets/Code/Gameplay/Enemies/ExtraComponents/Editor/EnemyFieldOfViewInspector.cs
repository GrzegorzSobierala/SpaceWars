using UnityEditor;
using UnityEngine;
using Game.Room.Enemy;

namespace Game.Editor
{
    [CustomEditor(typeof(EnemyFieldOfView))]
    public class EnemyFieldOfViewInspector : UnityEditor.Editor
    {
        private bool isDrawGizmosOn = false;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayoutOption[] options = new GUILayoutOption[0] ;
            if(GUILayout.Toggle(isDrawGizmosOn, "DrawGizmos", options))
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
