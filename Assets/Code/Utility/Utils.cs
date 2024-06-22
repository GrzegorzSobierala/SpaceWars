using Game.Room.Enemy;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Game.Utility
{
    public static class Utils
    {
        public static Vector3 WorldToScreenPointClamped(Vector3 position)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(position);

            screenPosition.x = Mathf.Clamp(screenPosition.x, 0f, Screen.width);
            screenPosition.y = Mathf.Clamp(screenPosition.y, 0f, Screen.height);

            return screenPosition;
        }

        /// <summary>
        /// Screan point to 2D phisic plane intersection point 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector2 ScreanPositionOn2DIntersection(Vector2 position) 
        {
            Ray ray = Camera.main.ScreenPointToRay(position);
            float t = -Camera.main.transform.position.z / ray.direction.z;
            return ray.origin + t * ray.direction;
        }

        /// <summary>
        /// Get angle from vector(1,0), return value from left is negative, value on the right is positive
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static float AngleDirected(Vector2 vector)
        {
            float angle = Mathf.Atan2(vector.y, vector.x);
            return angle * Mathf.Rad2Deg;
        }

        
        public static float AngleDirected(Vector2 startVectorPos , Vector2 endVectorPos)
        {
            return AngleDirected(endVectorPos - startVectorPos);
        }

        public static Vector2 RotateVector(Vector2 vector, float angleInDegrees)
        {
            float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(angleInRadians);
            float cos = Mathf.Cos(angleInRadians);

            float x = vector.x * cos - vector.y * sin;
            float y = vector.x * sin + vector.y * cos;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Calculate the dot product from an angle in degrees
        /// </summary>
        public static float AngleToDotProduct(float angleDegrees)
        {
            float angleRad = Mathf.Deg2Rad * angleDegrees;
            float dotProduct = Mathf.Cos(angleRad);

            return dotProduct;
        }


        public static void BindGetComponent<T>(DiContainer container) 
            where T : Component
        {
            GameObject gameObject = container.DefaultParent.gameObject;

            if (!gameObject.TryGetComponent(out T component))
            {
                string message = $"There is no {typeof(T)} on a GameObject: {gameObject.name}";
                Debug.LogError(message, gameObject);
                throw new System.NullReferenceException(message);
            }

            container.Bind<T>().FromInstance(component).AsSingle();
        }

        public static void BindComponentsInChildrens<T>(DiContainer container, bool includeInactive = true)
            where T : Component
        {
            GameObject gameObject = container.DefaultParent.gameObject;

            List<T> enemyFieldOfViews = gameObject.GetComponentsInChildren<T>(includeInactive).ToList();

            if (enemyFieldOfViews.Count == 0)
            {
                string message = $"There is no {typeof(T)} on a GameObject: {gameObject}";
                Debug.LogError(message, gameObject);
            }

            container.Bind<List<T>>().FromInstance(enemyFieldOfViews).AsSingle();
        }

        public static bool ContainsLayer(this LayerMask layerMask, int layer)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }
    }

    public static class Async
        {
            public static async System.Threading.Tasks.Task Wait(float seconds) => await System.Threading.Tasks.Task.Delay((int)(seconds * 1000));
            public static System.Runtime.CompilerServices.YieldAwaitable WaitAFrame => System.Threading.Tasks.Task.Yield();
        }

    public static class Vectors
        {
            public static float SqrDistance(Vector3 a, Vector3 b)
            {
                return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z).sqrMagnitude;
            }
        }

    public static class Lerp
        {
            public static IEnumerator SmoothFloat(float from, float to, float maxTime, Action<float> OnNewFloat, Action OnEnd = null, bool deltaTimeScaled = false)
            {
                float timer = 0;
                while (true)
                {
                    yield return null;
                    float t = timer / maxTime;
                    OnNewFloat?.Invoke(Mathf.Lerp(from, to, t));
                    timer += deltaTimeScaled ? Time.deltaTime : Time.unscaledDeltaTime;
                    if (timer >= maxTime)
                    {
                        OnNewFloat?.Invoke(to);
                        OnEnd?.Invoke();
                        yield break;
                    }
                }
            }
            public static IEnumerator SmoothFloat(float from, float to, float maxTime, LerpX.SmoothType smoothType, Action<float> OnNewFloat, Action OnEnd = null, bool deltaTimeScaled = false)
            {
                float timer = 0;
                while (true)
                {
                    yield return null;
                    float t = timer / maxTime;
                    t.Smooth(smoothType);
                    OnNewFloat?.Invoke(Mathf.Lerp(from, to, t));
                    timer += deltaTimeScaled ? Time.deltaTime : Time.unscaledDeltaTime;
                    if (timer >= maxTime)
                    {
                        t = 1;
                        t.Smooth(smoothType);
                        OnNewFloat?.Invoke(Mathf.Lerp(from, to, t));
                        OnEnd?.Invoke();
                        yield break;
                    }
                }
            }
            public static IEnumerator SmoothFloatWithStartPause(float from, float to, float maxTime, float pauseSeconds, Action<float> OnNewFloat, Action OnEnd = null, bool deltaTimeScaled = false)
            {
                float timer = 0;
                yield return new WaitForSecondsRealtime(pauseSeconds);
                while (true)
                {
                    yield return null;
                    float t = timer / maxTime;
                    OnNewFloat?.Invoke(Mathf.Lerp(from, to, t));
                    timer += deltaTimeScaled ? Time.deltaTime : Time.unscaledDeltaTime;
                    if (timer >= maxTime)
                    {
                        OnNewFloat?.Invoke(to);
                        OnEnd?.Invoke();
                        yield break;
                    }
                }
            }
            public static IEnumerator SmoothFloatWithStartPause(float from, float to, float maxTime, float pauseSeconds, LerpX.SmoothType smoothType, Action<float> OnNewFloat, Action OnEnd = null, bool deltaTimeScaled = false)
            {
                float timer = 0;
                yield return new WaitForSecondsRealtime(pauseSeconds);
                while (true)
                {
                    yield return null;
                    float t = timer / maxTime;
                    t.Smooth(smoothType);
                    OnNewFloat?.Invoke(Mathf.Lerp(from, to, t));
                    timer += deltaTimeScaled ? Time.deltaTime : Time.unscaledDeltaTime;
                    if (timer >= maxTime)
                    {
                        OnNewFloat?.Invoke(to);
                        OnEnd?.Invoke();
                        yield break;
                    }
                }
            }
            public static IEnumerator SmoothFloatWithStartEndPause(float from, float to, float maxTime, float pauseSecondsStart, int framesToPauseEnd, Action<float> OnNewFloat, Action OnEnd = null, bool deltaTimeScaled = false)
            {
                float timer = 0;
                yield return new WaitForSecondsRealtime(pauseSecondsStart);
                while (true)
                {
                    yield return null;
                    float t = timer / maxTime;
                    OnNewFloat?.Invoke(Mathf.Lerp(from, to, t));
                    timer += deltaTimeScaled ? Time.deltaTime : Time.unscaledDeltaTime;
                    if (timer >= maxTime)
                    {
                        OnNewFloat?.Invoke(to);
                        for (int i = 0; i < framesToPauseEnd; i++)
                        {
                            yield return null;
                        }
                        OnEnd?.Invoke();
                        yield break;
                    }
                }
            }

            public static IEnumerator SmoothVector(Vector3 from, Vector3 to, float maxTime, LerpX.SmoothType smoothType, Action<Vector3> OnNewPosition, Action OnEnd = null, bool deltaTimeScaled = false)
            {
                float timer = 0;
                while (true)
                {
                    yield return null;
                    float t = timer / maxTime;
                    t.Smooth(smoothType);
                    OnNewPosition?.Invoke(Vector3.Lerp(from, to, t));
                    timer += deltaTimeScaled ? Time.deltaTime : Time.unscaledDeltaTime;
                    if (timer >= maxTime)
                    {
                        OnNewPosition?.Invoke(to);
                        OnEnd?.Invoke();
                        yield break;
                    }
                }
            }
            public static IEnumerator SmoothVector(Vector3 from, Vector3 to, float maxTime, Action<Vector3> OnNewPosition, Action OnEnd = null, bool deltaTimeScaled = false)
            {
                float timer = 0;
                while (true)
                {
                    yield return null;
                    float t = timer / maxTime;
                    OnNewPosition?.Invoke(Vector3.Lerp(from, to, t));
                    timer += deltaTimeScaled ? Time.deltaTime : Time.unscaledDeltaTime;
                    if (timer >= maxTime)
                    {
                        OnNewPosition?.Invoke(to);
                        OnEnd?.Invoke();
                        yield break;
                    }
                }
            }

            public static IEnumerator SmoothVectorSlerp(Vector3 from, Vector3 to, float maxTime, LerpX.SmoothType smoothType, Action<Vector3> OnNewPosition, Action OnEnd = null, bool deltaTimeScaled = false)
            {
                float timer = 0;
                while (true)
                {
                    yield return null;
                    float t = timer / maxTime;
                    t.Smooth(smoothType);
                    OnNewPosition?.Invoke(Vector3.Slerp(from, to, t));
                    timer += deltaTimeScaled ? Time.deltaTime : Time.unscaledDeltaTime;
                    if (timer >= maxTime)
                    {
                        OnNewPosition?.Invoke(to);
                        OnEnd?.Invoke();
                        yield break;
                    }
                }
            }
            public static IEnumerator SmoothVectorSlerp(Vector3 from, Vector3 to, float maxTime, Action<Vector3> OnNewPosition, Action OnEnd = null, bool deltaTimeScaled = false)
            {
                float timer = 0;
                while (true)
                {
                    yield return null;
                    float t = timer / maxTime;
                    OnNewPosition?.Invoke(Vector3.Slerp(from, to, t));
                    timer += deltaTimeScaled ? Time.deltaTime : Time.unscaledDeltaTime;
                    if (timer >= maxTime)
                    {
                        OnNewPosition?.Invoke(to);
                        OnEnd?.Invoke();
                        yield break;
                    }
                }
            }

            public static IEnumerator SmoothQuaternion(Quaternion from, Quaternion to, float maxTime, LerpX.SmoothType smoothType, Action<Quaternion> OnNewPosition, Action OnEnd = null, bool deltaTimeScaled = false)
            {
                float timer = 0;
                while (true)
                {
                    yield return null;
                    float t = timer / maxTime;
                    t.Smooth(smoothType);
                    OnNewPosition?.Invoke(Quaternion.Slerp(from, to, t));
                    timer += deltaTimeScaled ? Time.deltaTime : Time.unscaledDeltaTime;
                    if (timer >= maxTime)
                    {
                        OnNewPosition?.Invoke(to);
                        OnEnd?.Invoke();
                        yield break;
                    }
                }
            }
            public static IEnumerator SmoothQuaternion(Quaternion from, Quaternion to, float maxTime, Action<Quaternion> OnNewPosition, Action OnEnd = null, bool deltaTimeScaled = false)
            {
                float timer = 0;
                while (true)
                {
                    yield return null;
                    float t = timer / maxTime;
                    OnNewPosition?.Invoke(Quaternion.Slerp(from, to, t));
                    timer += deltaTimeScaled ? Time.deltaTime : Time.unscaledDeltaTime;
                    if (timer >= maxTime)
                    {
                        OnNewPosition?.Invoke(to);
                        OnEnd?.Invoke();
                        yield break;
                    }
                }
            }


        }

    public static class Mathfs
        {
            /// <summary>
            /// Clamped value, use RemapUnclamped for unclumped return values
            /// </summary>
            public static float Remap(float initialFrom, float initialTo, float targetFrom, float targetTo, float interpolator)
            {
                float t = Mathf.InverseLerp(initialFrom, initialTo, interpolator);
                return Mathf.Lerp(targetFrom, targetTo, t);
            }
            public static float RemapUnclamped(float initialFrom, float initialTo, float targetFrom, float targetTo, float interpolator)
            {
                float t = InverseLerpUnclamped(initialFrom, initialTo, interpolator);
                return Mathf.LerpUnclamped(targetFrom, targetTo, t);
            }
            public static float InverseLerpUnclamped(float a, float b, float value) => (value - a) / (b - a);

            public static float Round(float value, float precision) => Mathf.Round(value / precision) * precision;


            private static Vector3 firstLerpPosition;
            private static Vector3 secondLerpPosition;
            private static Vector3 thirdLerpPosition;
            private static Vector3 fifthLerpPosition;
            private static Vector3 sixthLerpPosition;
            public static Vector3 QuadraticBezier(Vector3 A, Vector3 B, Vector3 C, float t)
            {
                firstLerpPosition = Vector3.Lerp(A, B, t);
                secondLerpPosition = Vector3.Lerp(B, C, t);

                return Vector3.Lerp(firstLerpPosition, secondLerpPosition, t);
            }
            public static Vector3 CubicBezier(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
            {
                firstLerpPosition = Vector3.Lerp(A, B, t);
                secondLerpPosition = Vector3.Lerp(B, C, t);
                thirdLerpPosition = Vector3.Lerp(C, D, t);
                fifthLerpPosition = Vector3.Lerp(firstLerpPosition, secondLerpPosition, t);
                sixthLerpPosition = Vector3.Lerp(secondLerpPosition, thirdLerpPosition, t);

                return Vector3.Lerp(fifthLerpPosition, sixthLerpPosition, t);
            }
        }

    public static class UIX
    {
        public static void SetLeft(this RectTransform rt, float left)
        {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }

        public static void SetRight(this RectTransform rt, float right)
        {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }

        public static void SetTop(this RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }

        public static void SetBottom(this RectTransform rt, float bottom)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }
    }

    public static class LerpX
    {
        public enum SmoothType { Smoothstep, Smootherstep, EaseOut, EaseIn, PingPongSmooth, EaseOutQuint, EaseOutCubic }
        public static void Smooth(this ref float t, SmoothType interpolate)
        {
            t = GetSmooth(ref t, interpolate);
        }
        public static float GetSmooth(this ref float t, SmoothType interpolate)
        {
            return interpolate switch
            {
                SmoothType.Smoothstep => t * t * (3f - 2f * t),
                SmoothType.Smootherstep => t * t * t * (t * (6f * t - 15f) + 10f),
                SmoothType.EaseOut => 1f - ((1 - t) * (1 - t) * (1 - t)),
                SmoothType.EaseIn => t * t * t,
                SmoothType.PingPongSmooth => Mathf.Sin(2 * (t - 0.25f) * Mathf.PI) * 0.5f + 0.5f,
                SmoothType.EaseOutQuint => 1 - (float)Math.Pow(1 - t, 5),
                SmoothType.EaseOutCubic => 1 - (float)Math.Pow(1 - t, 3),
                _ => t * t * (3f - 2f * t),
            };
        }
        public static float GetShockwaveFloat(this ref float t, float maxAmplitude = 0.3f, float frequency = 10, float tuneDownPower = 4)
        {
            return maxAmplitude * Mathf.Sin(Mathf.PI * t * frequency) * Mathf.Exp(-tuneDownPower * t);
        }
    }

    public static class GameObjectX
    {
        public static void SetLayerAll(this GameObject parent, int layer, bool skipParent = false)
        {
            if (!skipParent)
                parent.layer = layer;
            foreach (Transform child in parent.transform)
                child.gameObject.SetLayerAll(layer);
        }
        public static void SetTagRecursively(this GameObject parent, string tag)
        {
            foreach (Transform child in parent.transform)
                child.gameObject.tag = tag;
        }
    }

    public static class TransformX
    {
        public static void SetPosition(this Transform transform, Vector3 position) => transform.position = position;
        public static void SetRotation(this Transform transform, Quaternion rotation) => transform.rotation = rotation;

        #region LookAtClamped
        private static Vector3 lookAtForward;
        private static Vector3 lookAtRight;
        private static Vector3 lookAtUp;
        private static Vector3 targetDirection;
        private static Vector3 targetDirectionFlat;
        private static Vector3 clampToForwardFlat;
        private static float targetAngleForwardX;
        private static float targetAngleForwardY;
        /// <summary>
        /// LookAt with clamp on Y and X axes
        /// </summary>
        /// <param name="target">Target look at</param>
        /// <param name="forwardVectorClampTo">Forward vector you want to clamp to</param>
        /// <param name="limitAngleY">Angle limit left/right</param>
        /// <param name="limitAngleX">Angle limit up/down</param>
        /// <param name="showRays">Show debug ray?</param>
        public static void LookAtClamped(this Transform transform, Transform target, Vector3 forwardVectorClampTo, float limitAngleY, float limitAngleX, bool showRays = false)
        {
            void DrawRay(Vector3 direction, Color color)
            {
#if UNITY_EDITOR
                if (showRays)
                    Debug.DrawRay(transform.position, direction, color);
#endif
            }

            targetDirection = target.position - transform.position;
            targetDirectionFlat = targetDirection.Flat();
            clampToForwardFlat = forwardVectorClampTo.Flat();
            DrawRay(clampToForwardFlat, Color.yellow);
            DrawRay(targetDirection, Color.white);
            DrawRay(targetDirectionFlat, Color.black);

            //limit forward vector on Y axis
            targetAngleForwardY = Vector3.SignedAngle(clampToForwardFlat, targetDirectionFlat, Vector3.up);
            if (Mathf.Abs(targetAngleForwardY) > limitAngleY)
            {
                targetAngleForwardY = Mathf.Clamp(targetAngleForwardY, -limitAngleY, limitAngleY);
                targetDirectionFlat = Quaternion.AngleAxis(targetAngleForwardY, Vector3.up) * clampToForwardFlat;
            }
            DrawRay(targetDirectionFlat, Color.grey);

            //get right vector
            lookAtRight = Vector3.Cross(Vector3.up, targetDirectionFlat);
            if (lookAtRight.magnitude == 0)
            {
                //forward is upwards, right is right
                lookAtRight = Vector3.right;
            }
            lookAtRight.Normalize();
            DrawRay(lookAtRight, Color.red);

            //project forward vector
            lookAtForward = Vector3.ProjectOnPlane(targetDirection, lookAtRight);
            lookAtForward.Normalize();
            DrawRay(lookAtForward, Color.cyan);

            //limit forward vector on X axis
            targetAngleForwardX = Vector3.SignedAngle(targetDirectionFlat, lookAtForward, lookAtRight);
            if (Mathf.Abs(targetAngleForwardX) > limitAngleX)
            {
                targetAngleForwardX = Mathf.Clamp(targetAngleForwardX, -limitAngleX, limitAngleX);
                lookAtForward = Quaternion.AngleAxis(targetAngleForwardX, lookAtRight) * targetDirectionFlat;
            }
            DrawRay(lookAtForward, Color.blue);

            //get up vector
            lookAtUp = Vector3.Cross(lookAtForward, lookAtRight);
            DrawRay(lookAtUp, Color.green);

            transform.rotation = Quaternion.LookRotation(lookAtForward, lookAtUp);
        }
        /// <summary>
        /// LookAt with clamp on Y and X axes, returning current tilt angles
        /// </summary>
        /// <param name="target">Target look at</param>
        /// <param name="forwardVectorClampTo">Forward vector you want to clamp to</param>
        /// <param name="limitAngleY">Angle limit left/right</param>
        /// <param name="limitAngleX">Angle limit up/down</param>
        /// <param name="showRays">Show debug ray?</param>
        public static (float xTilt, float yTilt) LookAtClampedTilt(this Transform transform, Transform target, Vector3 forwardVectorClampTo, float limitAngleY, float limitAngleX, bool showRays = false)
        {
            transform.LookAtClamped(target, forwardVectorClampTo, limitAngleY, limitAngleX, showRays);
            return (targetAngleForwardX, targetAngleForwardY);
        }
        #endregion
    }

    public static class VectorX
    {
        public static void UpdatePositionCloserToCamera(this ref Vector3 worldPosition, ref Camera camera)
        {
            if (camera == null)
                camera = Camera.main;
            Vector3 dirToCamera = (worldPosition - camera.transform.position).normalized;
            worldPosition -= dirToCamera * 0.05f;
        }

        public static Vector2 VectorXZ(this Vector3 vector) => new Vector2(vector.x, vector.z);

        public static Vector3 Flat(this Vector3 vector)
        {
            vector.y = 0;
            return vector.normalized;
        }
    }

    public static class QuaternionX
    {
        public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
        {
            if (Time.deltaTime < Mathf.Epsilon) return rot;
            // account for double-cover
            var Dot = Quaternion.Dot(rot, target);
            var Multi = Dot > 0f ? 1f : -1f;
            target.x *= Multi;
            target.y *= Multi;
            target.z *= Multi;
            target.w *= Multi;
            // smooth damp (nlerp approx)
            var Result = new Vector4(
                Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
                Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
                Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
                Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
            ).normalized;

            // ensure deriv is tangent
            var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
            deriv.x -= derivError.x;
            deriv.y -= derivError.y;
            deriv.z -= derivError.z;
            deriv.w -= derivError.w;

            return new Quaternion(Result.x, Result.y, Result.z, Result.w);
        }
    }

    public static class ActionX
    {
        /// <summary>
        /// Slow delegate DynamicInvoke, use with caution!
        /// </summary>
        public static void TryCatchInvoke(this Action action)
        {
            Delegate[] calls = action.GetInvocationList();
            foreach (var call in calls)
            {
                try
                {
                    call.DynamicInvoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error on {action}!!! Method: {call.Method}. On object: {call.Target}");
                    Debug.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Slow delegate DynamicInvoke, use with caution!
        /// </summary>
        public static void TryCatchInvoke<T>(this Action<T> action, T param)
        {
            Delegate[] calls = action.GetInvocationList();
            foreach (var call in calls)
            {
                try
                {
                    call.DynamicInvoke(param);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error on {action}!!! Method: {call.Method}. On object: {call.Target}");
                    Debug.LogException(ex);
                }
            }
        }
    }

    public static class RandomX
    {
        public static int GetSystemRandom(int minInclusive, int maxExclusive)
        {
            System.Random random = new System.Random();
            return random.Next(minInclusive, maxExclusive);
        }
    }
}
