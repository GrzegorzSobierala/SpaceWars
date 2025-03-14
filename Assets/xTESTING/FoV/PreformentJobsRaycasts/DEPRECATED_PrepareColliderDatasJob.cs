using Game.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Physics
{
    [BurstCompile]
    public struct DEPRECATED_PrepareColliderDatasJob : IJobFor
    {
        [ReadOnly] public NativeList<ColliderDataUnprepared> datasUnprep;
        [ReadOnly] public NativeList<Vector2> vertsUnprep;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ColliderDataReady> datasRdy;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<float2> vertsRdy;

        public void Execute(int index)
        {
            switch (datasUnprep[index].typeEnum)
            {
                case ColliderType.Box:
                    {
                        ColliderDataReady data = new()
                        {
                            type = (int)ColliderType.Box,

                            // Compute world center using the collider’s offset.
                            center = (Vector2)datasUnprep[index].posWorld +
                                (Vector2)(Quaternion.Euler(0, 0, datasUnprep[index].rotWorld)
                                * datasUnprep[index].offsetLoc),

                            rotationRad = math.radians(datasUnprep[index].rotWorld),

                            size = new float2(datasUnprep[index].sizeLoc.x *
                                datasUnprep[index].lossyScale.x,
                            datasUnprep[index].sizeLoc.y * datasUnprep[index].lossyScale.y)
                        };

                        datasRdy[index] = data;
                        break;
                    }
                case ColliderType.Circle:
                    {
                        ColliderDataReady data = new()
                        {
                            type = (int)ColliderType.Circle,

                            center = (Vector2)datasUnprep[index].posWorld +
                                (Vector2)(Quaternion.Euler(0, 0, datasUnprep[index].rotWorld)
                                * datasUnprep[index].offsetLoc),

                            // Assume uniform scale (using the x component).
                            radius = datasUnprep[index].radiusLoc * datasUnprep[index].lossyScale.x
                        };

                        datasRdy[index] = data;
                        break;
                    }
                case ColliderType.Capsule:
                    {
                        // Compute world center.
                        Vector2 worldPos = (Vector2)datasUnprep[index].posWorld +
                        (Vector2)(Quaternion.Euler(0, 0, datasUnprep[index].rotWorld)
                        * datasUnprep[index].offsetLoc);

                        // Get lossy scale.
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
                            type = (int)ColliderType.Capsule,

                            capsuleRadius = capsuleRadius,
                            capsuleA = worldPos + (Vector2)datasUnprep[index].capsuleTransUp * segment,
                            capsuleB = worldPos - (Vector2)datasUnprep[index].capsuleTransUp * segment
                        };

                        datasRdy[index] = data;
                        break;
                    }
                case ColliderType.Polygon:
                    {
                        ColliderDataReady data = new()
                        {
                            type = (int)ColliderType.Polygon,
                            vertexStartIndex = datasUnprep[index].vertexStartIndex,
                            vertexCount = datasUnprep[index].vertexCount,
                            isClosed = 1,
                        };

                        // poly.points are in local space; transform them to world space.
                        Vector2 worldPos = datasUnprep[index].posWorld;
                        float worldAngle = datasUnprep[index].rotWorld;
                        Vector2 loosyScale = datasUnprep[index].lossyScale;

                        for (int i = datasUnprep[index].vertexStartIndex;
                            i < datasUnprep[index].vertexStartIndex + datasUnprep[index].vertexCount; 
                            i++)
                        {
                            vertsRdy[i] = Utils.TransformPoint(vertsUnprep[i], worldPos, worldAngle, 
                                loosyScale);
                        }

                        datasRdy[index] = data;
                        break;
                    }
                case ColliderType.Edge:
                    {
                        ColliderDataReady data = new()
                        {
                            type = (int)ColliderType.Edge,
                            vertexStartIndex = datasUnprep[index].vertexStartIndex,
                            vertexCount = datasUnprep[index].vertexCount,
                            isClosed = 0, // Edge is open.
                        };

                        for (int i = datasUnprep[index].vertexStartIndex;
                            i < datasUnprep[index].vertexStartIndex + datasUnprep[index].vertexCount; 
                            i++)
                        {
                            vertsRdy[i] = Utils.TransformPoint(vertsUnprep[i], 
                                datasUnprep[index].posWorld, datasUnprep[index].rotWorld,
                                datasUnprep[index].lossyScale);
                        }

                        datasRdy[index] = data;
                        break;
                    }
                case ColliderType.Composite:
                    {
                        // CompositeCollider2D may contain multiple paths. Add one ColliderData per path.
                        // IMPORTANT: To avoid applying the scale twice, do not use TransformPoint here.
                        ColliderDataReady data = new()
                        {
                            type = (int)ColliderType.Composite,
                            vertexStartIndex = datasUnprep[index].vertexStartIndex,
                            vertexCount = datasUnprep[index].vertexCount,
                            // Assume composite shapes are closed.
                            isClosed = 1,
                        };

                        Quaternion rot = Quaternion.Euler(0, 0, datasUnprep[index].rotWorld);

                        for (int i = datasUnprep[index].vertexStartIndex;
                            i < datasUnprep[index].vertexStartIndex + datasUnprep[index].vertexCount;
                            i++)
                        {
                            vertsRdy[i] = (Vector2)datasUnprep[index].posWorld +
                                (Vector2)(rot * vertsUnprep[i]);
                        }

                        datasRdy[index] = data;
                        break;
                    }
                default:
                    {
                        // FALLBACK: use bounds as a box.
                        ColliderDataReady data = new()
                        {
                            type = (int)ColliderType.Box,
                            center = new float2(datasUnprep[index].posWorld.x, 
                                datasUnprep[index].posWorld.y),
                            size = new float2(datasUnprep[index].sizeLoc.x, 
                                datasUnprep[index].sizeLoc.y),
                        };

                        datasRdy[index] = data;
                        break;
                    }
            }
        }
    }
}
