using UnityEditor;
using UnityEngine;

namespace Game
{
    public class RandomRotator
    {
        [MenuItem("SpaceWars/Random Rotate #r", false, -1)]
        private static void RandomRotate()
        {
            foreach (var gameObject in Selection.gameObjects)
            {
                RandomRotateObject(gameObject);
            }
        }

        private static void RandomRotateObject(GameObject gameObject)
        {
            float x = Random.Range(0f, 180f);
            float y = Random.Range(0f, 180f);
            float z = Random.Range(0f, 180f);

            gameObject.transform.rotation = Quaternion.Euler(x, y, z);
            EditorUtility.SetDirty(gameObject);
        }
    }
}
