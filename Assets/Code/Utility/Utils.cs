using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using UnityEngine.SceneManagement;
using System.Reflection;
using Unity.Mathematics;

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

        public static void BindGetComponent<T>(DiContainer container, GameObject gameObject, 
            bool nonLazy = false)
            
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
            bool includeInactive = true, bool enable0Count = false)
        {
            List<T> components = gameObject.GetComponentsInChildren<T>(includeInactive).ToList();

            if (!enable0Count && components.Count == 0)
            {
                string message = $"There is no {typeof(T)} on a GameObject: {gameObject}";
                Debug.LogError(message, gameObject);
            }

            container.Bind<List<T>>().FromInstance(components).AsSingle();
        }

        public static void BindComponentsInChildrensHash<T>(DiContainer container, GameObject gameObject,
           bool includeInactive = true, bool enable0Count = false)
        {
            var components = new HashSet<T>(gameObject.GetComponentsInChildren<T>(true));

            if (!enable0Count && components.Count == 0)
            {
                string message = $"There is no {typeof(T)} on a GameObject: {gameObject}";
                Debug.LogError(message, gameObject);
            }

            container.Bind<HashSet<T>>().FromInstance(components).AsSingle();
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

        public static Vector2 ChangeVector2Y(Vector2 toChange, float value)
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

        /// <summary>
        /// Remaps a value from one range to another.
        /// </summary>
        /// <param name="value">The value to remap.</param>
        /// <param name="sourceMin">The minimum value of the source range.</param>
        /// <param name="sourceMax">The maximum value of the source range.</param>
        /// <param name="destinationMin">The minimum value of the destination range.</param>
        /// <param name="destinationMax">The maximum value of the destination range.</param>
        /// <returns>The remapped value in the destination range.</returns>
        public static float Remap(float value, float sourceMin, float sourceMax,
            float destinationMin, float destinationMax)
        {
            return math.remap(sourceMin, sourceMax, destinationMin, destinationMax, value);
        }

        /// <summary>
        /// Calculates the world-space center of mass based on weighted centroids of all colliders attached to the Rigidbody2D.
        /// For area-based colliders, the weight is the computed area.
        /// For EdgeCollider2D, the weight is its total length.
        /// Supports: Circle, Box, Polygon, Edge, Capsule, and Composite Collider2D types.
        /// </summary>
        /// <param name="rb2D">The Rigidbody2D whose attached colliders will be used.</param>
        /// <returns>
        /// The calculated world center of mass. Returns Vector2.zero if no valid colliders are found.
        /// </returns>
        public static Vector2 CalculateWorldCenterOfMass(Rigidbody2D rb2D)
        {
            if (rb2D == null)
            {
                Debug.LogError("Rigidbody2D parameter is null.");
                return Vector2.zero;
            }

            // Gather all attached Collider2D components.
            List<Collider2D> colliders = new List<Collider2D>();
            rb2D.GetAttachedColliders(colliders);

            if (colliders.Count == 0)
            {
                Debug.LogError("No colliders attached to the Rigidbody2D.");
                return Vector2.zero;
            }

            float totalWeight = 0f;
            Vector2 weightedCentroidSum = Vector2.zero;

            foreach (Collider2D col in colliders)
            {
                if (col == null)
                    continue;

                if (TryCalculateWeightAndCentroid(col, out float weight, out Vector2 centroid))
                {
                    totalWeight += weight;
                    weightedCentroidSum += centroid * weight;
                }
                else
                {
                    Debug.LogWarning("Unsupported Collider2D type: " + col.GetType());
                }
            }

            if (totalWeight > 0f)
                return weightedCentroidSum / totalWeight;
            else
            {
                Debug.LogError("Total weight of colliders is zero. Cannot compute center of mass.");
                return Vector2.zero;
            }
        }

        /// <summary>
        /// Attempts to compute the weight (area or length) and world-space centroid for a given Collider2D.
        /// For area-based colliders, weight is the area.
        /// For an EdgeCollider2D, weight is its total length.
        /// </summary>
        private static bool TryCalculateWeightAndCentroid(Collider2D col, out float weight, out Vector2 centroid)
        {
            weight = 0f;
            centroid = Vector2.zero;
            Transform t = col.transform;

            // --- CircleCollider2D ---
            if (col is CircleCollider2D circle)
            {
                // Assume uniform scaling; using the x component.
                float worldRadius = circle.radius * Mathf.Abs(t.lossyScale.x);
                weight = Mathf.PI * worldRadius * worldRadius;
                centroid = (Vector2)t.TransformPoint(circle.offset);
                return true;
            }
            // --- BoxCollider2D ---
            else if (col is BoxCollider2D box)
            {
                Vector2 worldSize = new Vector2(
                    box.size.x * Mathf.Abs(t.lossyScale.x),
                    box.size.y * Mathf.Abs(t.lossyScale.y));
                weight = worldSize.x * worldSize.y;
                centroid = (Vector2)t.TransformPoint(box.offset);
                return true;
            }
            // --- PolygonCollider2D ---
            else if (col is PolygonCollider2D poly)
            {
                float totalPolyArea = 0f;
                Vector2 weightedPolyCentroid = Vector2.zero;

                for (int p = 0; p < poly.pathCount; p++)
                {
                    Vector2[] points = poly.GetPath(p);
                    float polyArea = 0f;
                    Vector2 polyCentroid = Vector2.zero;
                    int numPoints = points.Length;

                    // Calculate area and centroid in local space via the shoelace formula.
                    for (int i = 0; i < numPoints; i++)
                    {
                        Vector2 current = points[i];
                        Vector2 next = points[(i + 1) % numPoints];
                        float cross = current.x * next.y - next.x * current.y;
                        polyArea += cross;
                        polyCentroid += (current + next) * cross;
                    }
                    polyArea *= 0.5f;
                    if (Mathf.Approximately(polyArea, 0f))
                        continue;
                    polyCentroid /= (6f * polyArea);

                    float absArea = Mathf.Abs(polyArea);
                    totalPolyArea += absArea;
                    // Transform local centroid to world space.
                    Vector2 worldPolyCentroid = (Vector2)t.TransformPoint(polyCentroid);
                    weightedPolyCentroid += worldPolyCentroid * absArea;
                }

                if (totalPolyArea > 0f)
                {
                    weight = totalPolyArea;
                    centroid = weightedPolyCentroid / totalPolyArea;
                    return true;
                }
                return false;
            }
            // --- EdgeCollider2D ---
            else if (col is EdgeCollider2D edge)
            {
                Vector2[] points = edge.points;
                if (points.Length < 2)
                    return false;
                float totalLength = 0f;
                Vector2 weightedCentroid = Vector2.zero;
                // EdgeCollider2D points are defined in local space relative to the collider's offset.
                for (int i = 0; i < points.Length - 1; i++)
                {
                    Vector2 p0 = (Vector2)t.TransformPoint(points[i] + edge.offset);
                    Vector2 p1 = (Vector2)t.TransformPoint(points[i + 1] + edge.offset);
                    float segmentLength = Vector2.Distance(p0, p1);
                    totalLength += segmentLength;
                    weightedCentroid += ((p0 + p1) * 0.5f) * segmentLength;
                }
                if (totalLength > 0f)
                {
                    // Using length as the weight.
                    weight = totalLength;
                    centroid = weightedCentroid / totalLength;
                    return true;
                }
                return false;
            }
            // --- CapsuleCollider2D ---
            else if (col is CapsuleCollider2D capsule)
            {
                Vector2 size = capsule.size;
                Vector2 worldSize = new Vector2(
                    size.x * Mathf.Abs(t.lossyScale.x),
                    size.y * Mathf.Abs(t.lossyScale.y));
                // The capsule is symmetric; its centroid is the transformed offset.
                centroid = (Vector2)t.TransformPoint(capsule.offset);

                float capsuleArea = 0f;
                if (capsule.direction == CapsuleDirection2D.Vertical)
                {
                    float r = worldSize.x / 2f;
                    float rectHeight = Mathf.Max(0, worldSize.y - 2 * r);
                    float rectArea = worldSize.x * rectHeight;
                    float circleArea = Mathf.PI * r * r;
                    capsuleArea = rectArea + circleArea;
                }
                else // Horizontal
                {
                    float r = worldSize.y / 2f;
                    float rectWidth = Mathf.Max(0, worldSize.x - 2 * r);
                    float rectArea = worldSize.y * rectWidth;
                    float circleArea = Mathf.PI * r * r;
                    capsuleArea = rectArea + circleArea;
                }
                weight = capsuleArea;
                return true;
            }
            // --- CompositeCollider2D ---
            else if (col is CompositeCollider2D composite)
            {
                int pathCount = composite.pathCount;
                float totalCompositeArea = 0f;
                Vector2 weightedCompositeCentroid = Vector2.zero;
                List<Vector2> points = new List<Vector2>();
                for (int i = 0; i < pathCount; i++)
                {
                    points.Clear();
                    composite.GetPath(i, points);
                    if (points.Count < 3)
                        continue;
                    float polyArea = 0f;
                    Vector2 polyCentroid = Vector2.zero;
                    int numPoints = points.Count;
                    for (int j = 0; j < numPoints; j++)
                    {
                        Vector2 current = points[j];
                        Vector2 next = points[(j + 1) % numPoints];
                        float cross = current.x * next.y - next.x * current.y;
                        polyArea += cross;
                        polyCentroid += (current + next) * cross;
                    }
                    polyArea *= 0.5f;
                    if (Mathf.Approximately(polyArea, 0f))
                        continue;
                    polyCentroid /= (6f * polyArea);
                    float absArea = Mathf.Abs(polyArea);
                    totalCompositeArea += absArea;
                    Vector2 worldPolyCentroid = (Vector2)t.TransformPoint(polyCentroid);
                    weightedCompositeCentroid += worldPolyCentroid * absArea;
                }
                if (totalCompositeArea > 0f)
                {
                    weight = totalCompositeArea;
                    centroid = weightedCompositeCentroid / totalCompositeArea;
                    return true;
                }
                return false;
            }

            // Collider type not supported.
            return false;
        }

        /// <summary>
        /// Transforms a local point into world space by applying scale, rotation, and translation.
        /// </summary>
        /// <param name="localPoint">The local space point to transform.</param>
        /// <param name="worldPos">The translation (world position) to add.</param>
        /// <param name="worldAngle">The rotation angle (in degrees) to apply.</param>
        /// <param name="loosyScale">The scale to apply.</param>
        /// <returns>The transformed point in world space.</returns>
        public static Vector2 TransformPoint(Vector2 localPoint, Vector2 worldPos, float worldAngle, Vector2 loosyScale)
        {
            // First apply the scale to the local point
            Vector2 scaled = new Vector2(localPoint.x * loosyScale.x, localPoint.y * loosyScale.y);

            // Convert angle from degrees to radians for rotation
            float rad = worldAngle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            // Rotate the scaled point
            Vector2 rotated = new Vector2(
                scaled.x * cos - scaled.y * sin,
                scaled.x * sin + scaled.y * cos
            );

            // Finally, translate by world position
            return worldPos + rotated;
        }

        /// <summary>
        /// Transforms a local point into world space by applying rotation and translation.
        /// </summary>
        /// <param name="localPoint">The local space point to transform.</param>
        /// <param name="worldPos">The translation (world position) to add.</param>
        /// <param name="worldAngle">The rotation angle (in degrees) to apply.</param>
        /// <returns>The transformed point in world space.</returns>
        public static Vector2 TransformPoint(Vector2 localPoint, Vector2 worldPos, float worldAngle)
        {
            // Convert angle from degrees to radians for rotation
            float rad = worldAngle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            // Rotate the local point
            Vector2 rotated = new Vector2(
                localPoint.x * cos - localPoint.y * sin,
                localPoint.x * sin + localPoint.y * cos
            );

            // Finally, translate by world position
            return worldPos + rotated;
        }

        /// <summary>
        /// Rotates the transform to face the specified world position.
        /// </summary>
        /// <param name="transform">The transform to rotate.</param>
        /// <param name="worldPosition">The target world position to face.</param>
        /// <returns>The angle in degrees by which the transform was rotated.</returns>
        public static float RotateTowards(Transform transform, Vector2 worldPosition)
        {
            // Get the current position of the object as a Vector2.
            Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);

            // Calculate the direction from the current position to the target world position.
            Vector2 direction = worldPosition - currentPosition;

            // Calculate the angle in degrees using the arctan of the direction.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;

            // Create a quaternion that represents a rotation around the z-axis by the calculated angle.
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // Apply the rotation to the transform.
            transform.rotation = rotation;

            return angle;
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