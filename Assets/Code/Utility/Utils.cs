using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using UnityEngine.SceneManagement;
using System.Reflection;

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

        public static Ray ScreenPointToRay(Camera camera, Vector2 screenPoint)
        {
            // Step 1: Generate the inverse matrix
            Matrix4x4 inverseMatrix = (camera.projectionMatrix * camera.worldToCameraMatrix).inverse;

            // Step 2: Convert screen space pixel to clip space
            Vector2 clipPoint = new Vector2(
                (screenPoint.x / camera.pixelWidth) * 2f - 1f,
                (screenPoint.y / camera.pixelHeight) * 2f - 1f
            );

            // Clip space coordinates in 3D (near plane, z = -1 in clip space)
            Vector4 clipSpacePoint = new Vector4(clipPoint.x, clipPoint.y, -1f, 1f);

            // Step 3: Transform clip space point to world space
            Vector4 worldSpacePoint = inverseMatrix.MultiplyPoint(clipSpacePoint);

            // Homogeneous division to convert from 4D to 3D
            if (Mathf.Abs(worldSpacePoint.w) > Mathf.Epsilon)
            {
                worldSpacePoint /= worldSpacePoint.w;
            }

            // Step 4: Calculate ray direction
            Vector3 rayDirection = (new Vector3(worldSpacePoint.x, worldSpacePoint.y, worldSpacePoint.z) - camera.transform.position).normalized;

            // Step 5: Construct and return the ray
            return new Ray(camera.transform.position, rayDirection);
        }

        /// <summary>
        /// Get angle from vector(0,1), return value from left is negative, value on the right is positive
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static float AngleDirected(Vector2 vector)
        {
            return Vector2.SignedAngle(Vector2.up, vector);
        }

        public static float AngleDirected(Vector2 startVectorPos, Vector2 endVectorPos)
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

        public static Vector2 LocalToWorldDirection(Vector2 direction, Transform transform)
        {
            Vector3 localDirection3D = new Vector3(direction.x, direction.y, 0);
            Vector3 worldDirection3D = transform.TransformDirection(localDirection3D);
            Vector2 worldDirection = new Vector2(worldDirection3D.x, worldDirection3D.y);
            return worldDirection;
        }

        public static void BindGetComponent<T>(DiContainer container, GameObject gameObject, bool nonLazy = false)
            where T : Component
        {
            if (!gameObject.TryGetComponent(out T component))
            {
                string message = $"There is no {typeof(T)} on a GameObject: {gameObject.name}";
                Debug.LogError(message, gameObject);
                throw new System.NullReferenceException(message);
            }

            if (nonLazy)
            {
                container.Bind<T>().FromInstance(component).AsSingle().NonLazy();
            }
            else
            {
                container.Bind<T>().FromInstance(component).AsSingle();
            }
        }

        public static void BindComponentsInChildrens<T>(DiContainer container, GameObject gameObject, 
            bool includeInactive = true, bool enable0Count = false) where T : Component
        {
            List<T> enemyFieldOfViews = gameObject.GetComponentsInChildren<T>(includeInactive).ToList();

            if (enable0Count && enemyFieldOfViews.Count == 0)
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

        public static T FindEnemyInParents<T>(Transform transform, 
            int searchIterationLimit = 100) where T : MonoBehaviour

        {
            Transform currentParent = transform;
            for (int i = 0; i < searchIterationLimit; i++)
            {
                if (currentParent.TryGetComponent(out T enemyBase))
                {
                    return enemyBase;
                }

                if (currentParent.parent == null)
                    return null;

                currentParent = currentParent.parent;
            }

            return null;
        }

        /// <summary>
        /// Convert angle to format from -180 to 180
        /// </summary>
        public static float GetAngleIn180Format(float angle)
        {
            angle = angle % 360; // First, reduce to the range -360 to 360
            if (angle > 180)
                angle -= 360; // If greater than 180, shift down
            else if (angle < -180)
                angle += 360; // If less than -180, shift up
            return angle;
        }


        /// <summary>
        /// Calculates the time required to perform a rotation.
        /// </summary>
        /// <param name="rotationSpeed">Rotation speed in degrees per second.</param>
        /// <param name="rotationAngle">The angle of rotation in degrees.</param>
        /// <returns>The time required to perform the rotation in seconds.</returns>
        public static float CalculateRotationTime(float rotationSpeed, float rotationAngle)
        {
            if (rotationSpeed <= 0)
            {
                throw new ArgumentException("Rotation speed must be greater than zero.", nameof(rotationSpeed));
            }

            return Mathf.Abs(rotationAngle) / rotationSpeed;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/1073606/is-there-a-one-line-function-that-generates-a-triangle-wave
        /// https://www.desmos.com/calculator/bjqsoeulqi
        /// </summary>
        public static float TriangularFunc(float x, float amplitude, float halfPeriod, float moveY)
        {
            if(x < 0)
            {
                x = ModNormalised(x, halfPeriod * 2);
            }

            return amplitude / halfPeriod
                * (halfPeriod - Mathf.Abs((x % (2 * halfPeriod)) - halfPeriod)) - moveY;
        }

        /// <summary>
        /// Get the possible Xmoves for (x - XMove) in Triangular Function for provided y 
        /// </summary>
        /// <returns>(increasing,decreasing)</returns>
        public static (float, float) GetXMoveTriangularFunc(float x, float y, float amplitude, float halfPeriod, 
            float moveY)
        {
            float period = halfPeriod * 2;
            float modX = x < 0 ? ModNormalised(x,period) : x % period;

            float absValue = halfPeriod - halfPeriod * (y + moveY) / amplitude;

            if (absValue < 0 || absValue > halfPeriod)
            {
                Debug.LogError("Invalid parameters: resulting absValue is out of bounds.");
                return (float.NaN, float.NaN);
            }

            float S1 = modX - (halfPeriod + absValue);
            float S2 = modX - (halfPeriod - absValue);

            float S1x = ModNormalised(x - S1, period);

            if(S1x < halfPeriod)
            {
                return (S1, S2);
            }
            else
            {
                return (S2, S1);
            }
        }

        public static float ModNormalised(float value, float period)
        {
            return (value % period + period) % period;
        }

        public static Quaternion ChangeRotationZ(Quaternion rot, float newZ)
        {
            Vector3 newRot = new Vector3(rot.eulerAngles.x, rot.eulerAngles.y, newZ);
            return Quaternion.Euler(newRot);
        }

        public static Vector3 ChangeVector3X(Vector3 toChange, float value)
        {
            return new Vector3(value, toChange.y, toChange.z);
        }

        public static Vector3 ChangeVector3Y(Vector3 toChange, float value)
        {
            return new Vector3(toChange.x, value, toChange.z);
        }

        public static Vector3 ChangeVector3Z(Vector3 toChange, float value)
        {
            return new Vector3(toChange.x, toChange.y, value);
        }

        public static Vector2 ChangeVector2X(Vector2 toChange, float value)
        {
            return new Vector2(value, toChange.y);
        }

        public static Vector2 ChangeVector2Y(ref Vector2 toChange, float value)
        {
            return new Vector2(toChange.x, value);
        }

        public static Vector2 GetVector(Vector2 startPos, Vector2 endPos)
        {
            return endPos - startPos;
        }

        public static Vector3 GetVector(Vector3 startPos, Vector3 endPos)
        {
            return endPos - startPos;
        }

        /// <summary>
        /// Evaluates all subscribed methods in the given multicast delegate.
        /// </summary>
        /// <param name="combined">The multicast delegate of type Func&lt;bool&gt; containing methods to evaluate.</param>
        /// <returns>
        /// True if no methods are attached or false if any method returns false or true only if all subscribed methods return true.
        /// </returns>
        public static bool EvaluateCombinedFunc(Func<bool> combined)
        {
            if (combined == null)
                return true; // If no methods are attached, return false.

            // Get all subscribed methods and execute them in order.
            foreach (Func<bool> func in combined.GetInvocationList())
            {
                if (!func())
                {
                    // Short-circuit: if any method returns false, return false.
                    return false;
                }
            }
            // All methods returned true.
            return true;
        }

        /// <summary>
        /// Stops the given coroutine if it is running and sets the reference to null.
        /// </summary>
        /// <param name="mono">The MonoBehaviour instance that is running the coroutine.</param>
        /// <param name="coroutine">The reference to the coroutine to stop and clear.</param>
        public static void StopAndClearCoroutine(this MonoBehaviour mono, ref Coroutine coroutine)
        {
            if (coroutine == null)
                return;

            mono.StopCoroutine(coroutine);
            coroutine = null;
        }

        public enum SceneLoadingState
        {
            NotLoaded,
            Loading,
            Loaded,
            Unloading
        }

        public static SceneLoadingState? GetLoadingState(Scene scene)
        {
            Type sceneType = typeof(Scene);
            PropertyInfo loadingStateProp = sceneType.GetProperty("loadingState", BindingFlags.NonPublic | BindingFlags.Instance);

            SceneLoadingState? sceneLoadingState;
            if (loadingStateProp != null)
            {
                if(Enum.TryParse(loadingStateProp.GetValue(scene, null).ToString(),
                    out SceneLoadingState loadingState))
                {
                    sceneLoadingState = loadingState;
                }
                else
                {
                    Debug.LogError("loadingState is null.");
                    return null;
                }
            }
            else
            {
                Debug.LogError("loadingStateProp is null.");
                return null;
            }

            return sceneLoadingState;
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