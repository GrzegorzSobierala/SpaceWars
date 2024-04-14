using UnityEditor;
using UnityEngine;

namespace Game
{
    public class RandomRotator
    {
        [MenuItem("SpaceWars/Random Rotate All #r", false, -1)]
        private static void RandomRotateAll()
        {
            foreach (var gameObject in Selection.gameObjects)
            {
                RandomRotateObject(gameObject, false);
            }
        }

        [MenuItem("SpaceWars/Random Rotate Z #e", false, -1)]
        private static void RandomRotateZ()
        {
            foreach (var gameObject in Selection.gameObjects)
            {
                RandomRotateObject(gameObject, true);
            }
        }

        private static void RandomRotateObject(GameObject gameObject, bool onlyZ)
        {
            float x;
            float y;
            float z = Random.Range(0f, 180f);

            if (onlyZ)
            {
                x = gameObject.transform.rotation.eulerAngles.x;
                y = gameObject.transform.rotation.eulerAngles.y;
            }
            else
            {
                x = Random.Range(0f, 180f);
                y = Random.Range(0f, 180f);
            }
            

            gameObject.transform.rotation = Quaternion.Euler(x, y, z);
            EditorUtility.SetDirty(gameObject);
        }
    }
}
