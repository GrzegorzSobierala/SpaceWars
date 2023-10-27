using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game
{
    public class FpsCounter : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI textMesh;
        [SerializeField] private float interval = 1.0f; // Time interval in seconds to calculate average FPS
        [SerializeField] private int framesToAverage = 60; // Number of frames to average

        private float deltaTime = 0.0f;
        private float fpsAccumulator = 0;
        private int frameCounter = 0;
        private float currentFPS = 0;

        void Update()
        {
            deltaTime += Time.deltaTime;
            fpsAccumulator += 1.0f / Time.deltaTime;
            frameCounter++;

            // Calculate average FPS over the specified time interval
            if (deltaTime > interval)
            {
                currentFPS = fpsAccumulator / frameCounter;

                // Reset counters
                deltaTime = 0.0f;
                fpsAccumulator = 0;
                frameCounter = 0;

                textMesh.text = currentFPS.ToString("0.00");
            }
        }
    }
}
