using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game
{
    public class FpsCounter : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _textMesh;
        [SerializeField] private float _interval = 1.0f;

        private float deltaTime = 0.0f;
        private float fpsAccumulator = 0;
        private int frameCounter = 0;
        private float currentFPS = 0;

        void Update()
        {
            deltaTime += Time.deltaTime;
            fpsAccumulator += 1.0f / Time.deltaTime;
            frameCounter++;

            if (deltaTime > _interval)
            {
                currentFPS = fpsAccumulator / frameCounter;

                deltaTime = 0.0f;
                fpsAccumulator = 0;
                frameCounter = 0;

                _textMesh.text = currentFPS.ToString("0.00");
            }
        }
    }
}
