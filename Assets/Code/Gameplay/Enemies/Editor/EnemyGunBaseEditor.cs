using Game.Room.Enemy;
using Game.Utility;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    [CustomEditor(typeof(BasicEnemyGun))]
    public class EnemyGunBaseEditor : SpaceWarsInspector
    {
        private bool isDrawGizmosOn = true;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayoutOption[] options = new GUILayoutOption[0];
            BasicEnemyGun gun = (BasicEnemyGun)target;

            if (GUILayout.Toggle(isDrawGizmosOn, "Draw travers gizmos", options))
            {
                if(!isDrawGizmosOn)
                {
                    SceneView.RepaintAll();
                }

                isDrawGizmosOn = true;
            }
            else
            {
                if (isDrawGizmosOn)
                {
                    SceneView.RepaintAll();
                }

                isDrawGizmosOn = false;
            }
        }

        private void OnSceneGUI()
        {
            BasicEnemyGun gun = (BasicEnemyGun)target;

            if (isDrawGizmosOn)
            {
                Color color = Color.white;

                if(gun.GunTraverse < 0 || gun.GunTraverse > 360 )
                {
                    color = Color.red;
                }    

                float rayLenght = 100;

                Vector3 rightDir = Utils.RotateVector(gun.transform.up, gun.GunTraverse / 2);
                Debug.DrawRay(gun.transform.position, rightDir * rayLenght, color);

                Vector3 leftDir = Utils.RotateVector(gun.transform.up, -gun.GunTraverse / 2);
                Debug.DrawRay(gun.transform.position, leftDir * rayLenght, color);
            }
        }
    }
}
