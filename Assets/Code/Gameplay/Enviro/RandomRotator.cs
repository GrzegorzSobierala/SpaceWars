using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class RandomRotator : MonoBehaviour
    {
        [SerializeField] private bool _rotateOnAwake = true;

        private void Awake()
        {
            if (_rotateOnAwake)
            {
                RandomRotate();
            }
        }

        [ContextMenu(nameof(RandomRotate))]
        public void RandomRotate()
        {
            float x = Random.Range(0f, 180f);
            float y = Random.Range(0f, 180f);
            float z = Random.Range(0f, 180f);

            transform.rotation = Quaternion.Euler(x, y, z);
        }

        [ContextMenu(nameof(RandomRotateAllOnScene))]
        private void RandomRotateAllOnScene()
        {
            foreach (RandomRotator rotator in FindObjectsOfType<RandomRotator>())
            {
                rotator.RandomRotate();
            }
        }
    }
}
