using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Physics
{
    [BurstCompile]
    public struct PrepareColliderDatasJob : IJob
    {
        [ReadOnly] public NativeList<ColliderDataUnprepared> datasUnprep;
        [ReadOnly] public NativeList<float2> vertsUnprep;

        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeHashMap<int, ColliderDataReady> datasRdy;
        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeList<float2> vertsRdy;

        public void Execute()
        {
            for (int index = 0; index < datasUnprep.Length; index++)
            {
                switch (datasUnprep[index].typeEnum)
                {
                    case ColliderType.Box:
                        {
                            CreateBoxData(index);
                            break;
                        }
                    case ColliderType.Circle:
                        {
                            CreateCircleData(index);
                            break;
                        }
                    case ColliderType.Capsule:
                        {
                            CreateCapsuleData(index);
                            break;
                        }
                    case ColliderType.Polygon:
                        {
                            CreatePolygonData(index);
                            break;
                        }
                    case ColliderType.Edge:
                        {
                            CreateEdgeData(index);
                            break;
                        }
                    case ColliderType.Composite:
                        {
                            CreateCompositeData(index);
                            break;
                        }
                    default:
                        {
                            CreateBoxDataForUnsuported(index);
                            break;
                        }
                }

            }
        }

        private void CreateBoxData(int index)
        {
            float angleRad = math.radians(datasUnprep[index].rotWorld);
            float2 rotatedOffset = new float2(
                datasUnprep[index].offsetLoc.x * math.cos(angleRad) - datasUnprep[index].offsetLoc.y * math.sin(angleRad),
                datasUnprep[index].offsetLoc.x * math.sin(angleRad) + datasUnprep[index].offsetLoc.y * math.cos(angleRad)
            );
            ColliderDataReady data = new()
            {
                type = ColliderType.Box,
                center = datasUnprep[index].posWorld + rotatedOffset,
                rotationRad = angleRad,
                size = new float2(datasUnprep[index].sizeLoc.x *
                    datasUnprep[index].lossyScale.x,
                datasUnprep[index].sizeLoc.y * datasUnprep[index].lossyScale.y),

                colliderId = datasUnprep[index].colliderId,
                layer = datasUnprep[index].layer
            };

            datasRdy.TryAdd(datasUnprep[index].colliderId, data);
        }

        private void CreateCircleData(int index)
        {
            float angleRad = math.radians(datasUnprep[index].rotWorld);
            float2 rotatedOffset = new float2(
                datasUnprep[index].offsetLoc.x * math.cos(angleRad) - datasUnprep[index].offsetLoc.y * math.sin(angleRad),
                datasUnprep[index].offsetLoc.x * math.sin(angleRad) + datasUnprep[index].offsetLoc.y * math.cos(angleRad)
            );
            ColliderDataReady data = new()
            {
                type = ColliderType.Circle,

                center = datasUnprep[index].posWorld + rotatedOffset,

                // Assume uniform scale (using the x component).
                radius = datasUnprep[index].radiusLoc * datasUnprep[index].lossyScale.x,
                colliderId = datasUnprep[index].colliderId,
                layer = datasUnprep[index].layer
            };

            datasRdy.TryAdd(datasUnprep[index].colliderId, data);
        }

        private void CreateCapsuleData(int index)
        {
            float angleRad = math.radians(datasUnprep[index].rotWorld);
            float2 rotatedOffset = new float2(
                datasUnprep[index].offsetLoc.x * math.cos(angleRad) - datasUnprep[index].offsetLoc.y * math.sin(angleRad),
                datasUnprep[index].offsetLoc.x * math.sin(angleRad) + datasUnprep[index].offsetLoc.y * math.cos(angleRad)
            );
            float2 worldPos = datasUnprep[index].posWorld + rotatedOffset;

            float width;
            float height;
            if (datasUnprep[index].capsuleDirEnum == CapsuleDirection2D.Vertical)
            {
                width = datasUnprep[index].sizeLoc.x * datasUnprep[index].lossyScale.x;
                height = datasUnprep[index].sizeLoc.y * datasUnprep[index].lossyScale.y;
            }
            else
            {
                width = datasUnprep[index].sizeLoc.y * datasUnprep[index].lossyScale.y;
                height = datasUnprep[index].sizeLoc.x * datasUnprep[index].lossyScale.x;
            }
            float capsuleRadius = width * 0.5f;
            float segment = math.max(0f, height * 0.5f - capsuleRadius);

            ColliderDataReady data = new()
            {
                type = ColliderType.Capsule,

                capsuleRadius = capsuleRadius,
                capsuleAOrBoundsPos = worldPos + datasUnprep[index].capsuleTransUpOrBoundsPos * segment,
                capsuleBOrBoundsSize = worldPos - datasUnprep[index].capsuleTransUpOrBoundsPos * segment,
                colliderId = datasUnprep[index].colliderId,
                layer = datasUnprep[index].layer
            };

            datasRdy.TryAdd(datasUnprep[index].colliderId, data);
        }

        private void CreatePolygonData(int index)
        {
            ColliderDataReady data = new()
            {
                type = ColliderType.Polygon,
                vertexStartIndex = datasUnprep[index].vertexStartIndex,
                vertexCount = datasUnprep[index].vertexCount,
                isClosed = 1,
                capsuleAOrBoundsPos = datasUnprep[index].capsuleTransUpOrBoundsPos,
                capsuleBOrBoundsSize = datasUnprep[index].capsuleTransRightOrBoundsSize,
                colliderId = datasUnprep[index].colliderId,
                layer = datasUnprep[index].layer
            };

            // poly.points are in local space; transform them to world space.
            float2 worldPos = datasUnprep[index].posWorld;
            float worldAngle = datasUnprep[index].rotWorld;
            float2 loosyScale = datasUnprep[index].lossyScale;

            for (int i = datasUnprep[index].vertexStartIndex;
                i < datasUnprep[index].vertexStartIndex + datasUnprep[index].vertexCount;
                i++)
            {
                vertsRdy[i] = TransformPoint(vertsUnprep[i], worldPos, worldAngle,
                    loosyScale);
            }

            datasRdy.TryAdd(datasUnprep[index].colliderId, data);
        }

        private void CreateEdgeData(int index)
        {
            ColliderDataReady data = new()
            {
                type = ColliderType.Edge,
                vertexStartIndex = datasUnprep[index].vertexStartIndex,
                vertexCount = datasUnprep[index].vertexCount,
                isClosed = 0, // Edge is open.
                capsuleAOrBoundsPos = datasUnprep[index].capsuleTransUpOrBoundsPos,
                capsuleBOrBoundsSize = datasUnprep[index].capsuleTransRightOrBoundsSize,
                colliderId = datasUnprep[index].colliderId,
                layer = datasUnprep[index].layer
            };

            for (int i = datasUnprep[index].vertexStartIndex;
                i < datasUnprep[index].vertexStartIndex + datasUnprep[index].vertexCount;
                i++)
            {
                vertsRdy[i] = TransformPoint(vertsUnprep[i],
                    datasUnprep[index].posWorld, datasUnprep[index].rotWorld,
                    datasUnprep[index].lossyScale);
            }

            datasRdy.TryAdd(datasUnprep[index].colliderId, data);
        }

        private void CreateCompositeData(int index)
        {
            // CompositeCollider2D may contain multiple paths. Add one ColliderData per path.
            // IMPORTANT: To avoid applying the scale twice, do not use TransformPoint here.
            ColliderDataReady data = new()
            {
                type = ColliderType.Composite,
                vertexStartIndex = datasUnprep[index].vertexStartIndex,
                vertexCount = datasUnprep[index].vertexCount,
                // Assume composite shapes are closed.
                isClosed = 1,
                capsuleAOrBoundsPos = datasUnprep[index].capsuleTransUpOrBoundsPos,
                capsuleBOrBoundsSize = datasUnprep[index].capsuleTransRightOrBoundsSize,
                colliderId = datasUnprep[index].colliderId,
                layer = datasUnprep[index].layer
            };

            float angleRad = math.radians(datasUnprep[index].rotWorld);
            for (int i = datasUnprep[index].vertexStartIndex;
                i < datasUnprep[index].vertexStartIndex + datasUnprep[index].vertexCount;
                i++)
            {
                float2 rotatedOffset = new float2(
                    vertsUnprep[i].x * math.cos(angleRad) - vertsUnprep[i].y * math.sin(angleRad),
                    vertsUnprep[i].x * math.sin(angleRad) + vertsUnprep[i].y * math.cos(angleRad)
                );

                vertsRdy[i] = datasUnprep[index].posWorld + rotatedOffset;
            }

            datasRdy.TryAdd(datasUnprep[index].colliderId, data);
        }

        private void CreateBoxDataForUnsuported(int index)
        {
            // FALLBACK: use bounds as a box.
            ColliderDataReady data = new()
            {
                type = ColliderType.Box,
                center = new float2(datasUnprep[index].posWorld.x,
                    datasUnprep[index].posWorld.y),
                size = new float2(datasUnprep[index].sizeLoc.x,
                    datasUnprep[index].sizeLoc.y),

                colliderId = datasUnprep[index].colliderId,
                layer = datasUnprep[index].layer
            };

            datasRdy.TryAdd(datasUnprep[index].colliderId, data);
        }

        private static float2 TransformPoint(float2 localPoint, float2 worldPos, 
            float worldAngle, float2 loosyScale)
        {
            // Apply the scale to the local point.
            float2 scaled = localPoint * loosyScale;

            // Convert the angle from degrees to radians.
            float rad = math.radians(worldAngle);

            // Calculate cosine and sine of the angle.
            float cos = math.cos(rad);
            float sin = math.sin(rad);

            // Rotate the scaled point.
            float2 rotated = new float2(
                scaled.x * cos - scaled.y * sin,
                scaled.x * sin + scaled.y * cos
            );

            // Translate by world position.
            return worldPos + rotated;
        }
    }
}
