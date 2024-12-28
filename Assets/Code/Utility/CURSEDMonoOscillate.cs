using System;
using System.Collections;
using UnityEngine;

namespace Game.Utility
{
    public static class CURSEDMonoOscillate
    {
        public static void OscillateRot(this MonoBehaviour mono, float lowestAmplitude
            , float highestAmplitude, float rotSpeed, float startValue
            , Action<float> onOscillate, OscillateStart oscillateStart = OscillateStart.Random)
        {

            Func<IEnumerator> coroutine = () => OscillateRotCor(mono, lowestAmplitude, highestAmplitude
                , rotSpeed, startValue, oscillateStart, onOscillate);

            TryStartCoroutine(mono, coroutine);
        }



        public static void Oscillate(this MonoBehaviour mono, float lowestAmplitude
            , float highestAmplitude, float halfPeriod, float startValue
            , Action<float> onOscillate, OscillateStart oscillateStart = OscillateStart.Random)
        {
            Func<IEnumerator> coroutine = () => OscillateCor(mono, lowestAmplitude, highestAmplitude
                , halfPeriod , startValue, oscillateStart, onOscillate);

            TryStartCoroutine(mono, coroutine);
        }


        private static void TryStartCoroutine(MonoBehaviour mono, Func<IEnumerator> coroutine)
        {
            if (mono.IsInvoking(nameof(OscilateNextFrameInvokeMarker)))
            {
                Debug.LogWarning("Oscillate is already requested for this frame, returning...");
                return;
            }

            if (mono.IsInvoking(nameof(OscilationInvokeMarker)))
            {
                mono.Invoke(nameof(OscilateNextFrameInvokeMarker), float.MaxValue);
            }
            else
            {
                mono.Invoke(nameof(OscilationInvokeMarker), float.MaxValue);
                mono.Invoke(nameof(OscilateNextFrameInvokeMarker), float.MaxValue);
                mono.StartCoroutine(coroutine.Invoke());
            }
        }

        private static IEnumerator OscillateCor(MonoBehaviour mono, float lowestY,
            float highestY, float halfPeriod, float startY, OscillateStart oscillateStart
            , Action<float> onOscillate)
        {
            float amplitude = highestY - lowestY;
            float moveY = (highestY - lowestY) / 2f;
            float previousFrameTime = Time.time - Time.deltaTime;

            bool isStartIncreasing;
            switch (oscillateStart)
            {
                case OscillateStart.Random:
                    isStartIncreasing = UnityEngine.Random.Range(0, 2) == 0;
                    break;
                case OscillateStart.Increasing:
                    isStartIncreasing = true;
                    break;
                case OscillateStart.Decreasing:
                    isStartIncreasing = false;
                    break;
                default:
                    Debug.LogError("Switch error");
                    isStartIncreasing = true;
                    break;
            }

            float moveX;
            if (isStartIncreasing)
            {
                moveX = Utils.GetXMoveTriangularFunc(previousFrameTime, startY, 
                    amplitude, halfPeriod, moveY).Item1;
            }
            else
            {
                moveX = Utils.GetXMoveTriangularFunc(previousFrameTime, startY, 
                    amplitude, halfPeriod, moveY).Item2;
            }


            do
            {
                float targetX = Time.time - moveX;
                float currentY = Utils.TriangularFunc(targetX, amplitude, halfPeriod, moveY);
                onOscillate.Invoke(currentY);

                mono.CancelInvoke(nameof(OscilateNextFrameInvokeMarker));

                yield return null;
            }
            while (mono.IsInvoking(nameof(OscilateNextFrameInvokeMarker)));

            mono.CancelInvoke(nameof(OscilationInvokeMarker));
        }

        private static IEnumerator OscillateRotCor(MonoBehaviour mono, float lowestY,
           float highestY, float rotSpeed, float startY, OscillateStart oscillateStart
           , Action<float> onOscillate)
        {
            float amplitude = highestY - lowestY;
            float moveY = (highestY - lowestY) / 2f;
            float previousFrameTime = Time.time - Time.deltaTime;
            float halfPeriod = Utils.CalculateRotationTime(rotSpeed, amplitude);

            float moveX = GetMoveX(previousFrameTime,startY,amplitude,halfPeriod, moveY, oscillateStart);

            do
            {
                float targetX = Time.time - moveX;
                float currentY = Utils.TriangularFunc(targetX, amplitude, halfPeriod, moveY);
                onOscillate.Invoke(currentY);

                mono.CancelInvoke(nameof(OscilateNextFrameInvokeMarker));

                yield return null;
            }
            while (mono.IsInvoking(nameof(OscilateNextFrameInvokeMarker)));

            mono.CancelInvoke(nameof(OscilationInvokeMarker));
        }

        private static void OscilateNextFrameInvokeMarker() { }

        private static void OscilationInvokeMarker() { }

        private static float GetMoveX(float previousFrameTime, float startY,float amplitude, 
            float halfPeriod, float moveY, OscillateStart oscillateStart)
        {
            bool isStartIncreasing;
            switch (oscillateStart)
            {
                case OscillateStart.Random:
                    isStartIncreasing = UnityEngine.Random.Range(0, 2) == 0;
                    break;
                case OscillateStart.Increasing:
                    isStartIncreasing = true;
                    break;
                case OscillateStart.Decreasing:
                    isStartIncreasing = false;
                    break;
                default:
                    Debug.LogError("Switch error");
                    isStartIncreasing = true;
                    break;
            }

            float moveX;
            if (isStartIncreasing)
            {
                moveX = Utils.GetXMoveTriangularFunc(previousFrameTime, startY,
                    amplitude, halfPeriod, moveY).Item1;
            }
            else
            {
                moveX = Utils.GetXMoveTriangularFunc(previousFrameTime, startY,
                    amplitude, halfPeriod, moveY).Item2;
            }

            return moveX;
        }

        public enum OscillateStart
        {
            Random = 0,
            Increasing = 1, //left
            Decreasing = 2, //right
        }
    }
}
