using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Utility
{
    public class OscillateController
    {
        private float _amplitude;
        private float _moveY;
        private float _moveX;
        private float _halfPeriod;
        private int _lastOscillateFrame = -100;

        public float OscillateThisFrame(float lowestY, float highestY, float halfPeriod, float startY, 
            OscillateStart oscillateStart = OscillateStart.Random)
        {
            if(Time.frameCount == _lastOscillateFrame)
            {
                Debug.LogError("Already oscillate this frame");
                return startY;
            }
            else if (Time.frameCount - 1 > _lastOscillateFrame)
            {
                SetUpAmplitudeAndMoveY(lowestY, highestY);
                SetUpHalfPeriodAndMoveX(halfPeriod, startY, oscillateStart);
            }

            return Osciallate();
        }

        public float OscillateRotThisThisFrame(float lowestY, float highestY, float rotSpeed
            , float startY, OscillateStart oscillateStart = OscillateStart.Random)
        {
            if (Time.frameCount == _lastOscillateFrame)
            {
                Debug.LogError("Already oscillate this frame");
                return startY;
            }
            else if (Time.frameCount - 1 > _lastOscillateFrame)
            {
                SetUpAmplitudeAndMoveY(lowestY, highestY);
                float halfPeriod = Utils.CalculateRotationTime(rotSpeed, _amplitude);
                SetUpHalfPeriodAndMoveX(halfPeriod, startY, oscillateStart);
            }

            return Osciallate();
        }

        public float OscillateRotThisThisFrame(float travers, float rotSpeed
            , float startY, OscillateStart oscillateStart = OscillateStart.Random)
        {
            if (Time.frameCount == _lastOscillateFrame)
            {
                Debug.LogError("Already oscillate this frame");
                return startY;
            }
            else if (Time.frameCount - 1 > _lastOscillateFrame)
            {
                SetUpAmplitudeAndMoveY(travers);
                float halfPeriod = Utils.CalculateRotationTime(rotSpeed, _amplitude);
                SetUpHalfPeriodAndMoveX(halfPeriod, startY, oscillateStart);
            }

            return Osciallate();
        }

        private void SetUpAmplitudeAndMoveY(float lowestY, float highestY)
        {
            _amplitude = highestY - lowestY;
            _moveY = (highestY - lowestY) / 2f;
        }

        private void SetUpAmplitudeAndMoveY(float travers)
        {
            _amplitude = travers;
            _moveY = travers - (travers/2);
        }

        private void SetUpHalfPeriodAndMoveX(float halfPeriod, float startY
            , OscillateStart oscillateStart)
        {
            _halfPeriod = halfPeriod;
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

            if (isStartIncreasing)
            {
                _moveX = Utils.GetXMoveTriangularFunc(previousFrameTime, startY,
                    _amplitude, halfPeriod, _moveY).Item1;
            }
            else
            {
                _moveX = Utils.GetXMoveTriangularFunc(previousFrameTime, startY,
                    _amplitude, halfPeriod, _moveY).Item2;
            }
        }

        private float Osciallate()
        {
            float targetX = Time.time - _moveX;
            float result = Utils.TriangularFunc(targetX, _amplitude, _halfPeriod, _moveY);
            _lastOscillateFrame = Time.frameCount;
            return result;
        }

        public enum OscillateStart
        {
            Random = 0,
            Increasing = 1, //left
            Decreasing = 2, //right
        }
    }
}
