using Game.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Physics
{
    [BurstCompile]
    public struct PrepareColliderDatasJob : IJobFor
    {
        [ReadOnly] public NativeList<ColliderDataUnprepared> datasUnprep;
        [ReadOnly] public NativeList<Vector2> vertsUnprep;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ColliderDataReady> datasRdy;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<float2> vertsRdy;

        public void Execute(int index)
        {
            // BOX
            if (datasUnprep[index].typeEnum == ColliderType.Box)
            {
                ColliderDataReady data = new()
                {
                    type = (int)ColliderType.Box,

                    // Compute world center using the collider’s offset.
                    center = (Vector2)datasUnprep[index].posWorld +
                        (Vector2)(Quaternion.Euler(0, 0, datasUnprep[index].rotWorld)
                        * datasUnprep[index].offsetLoc),

                    rotationRad = math.radians(datasUnprep[index].rotWorld),

                    size = new float2(datasUnprep[index].sizeLoc.x * datasUnprep[index].lossyScale.x,
                    datasUnprep[index].sizeLoc.y * datasUnprep[index].lossyScale.y)
                };

                datasRdy[index] = data;
            }
            // CIRCLE
            else if (datasUnprep[index].typeEnum == ColliderType.Circle)
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
            }
            // CAPSULE
            else if (datasUnprep[index].typeEnum == ColliderType.Capsule)
            {
                // Compute world center.
                Vector2 worldCenter = (Vector2)datasUnprep[index].posWorld +
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
                    capsuleA = worldCenter + (Vector2)datasUnprep[index].capsuleTransUp * segment,
                    capsuleB = worldCenter - (Vector2)datasUnprep[index].capsuleTransUp * segment
                };

                datasRdy[index] = data;
            }
            // POLYGON
            else if (datasUnprep[index].typeEnum == ColliderType.Polygon)
            {
                ColliderDataReady data = new()
                {
                    type = (int)ColliderType.Polygon,
                    vertexStartIndex = datasUnprep[index].vertexStartIndex,
                    vertexCount = datasUnprep[index].vertexCount,
                    isClosed = datasUnprep[index].isClosedBool ? 1 : 0,
                };

                // poly.points are in local space; transform them to world space.
                Vector2 worldPos = datasUnprep[index].posWorld;
                float worldAngle = datasUnprep[index].rotWorld;
                Vector2 loosyScale = datasUnprep[index].lossyScale;

                for (int i = datasUnprep[index].vertexStartIndex;
                    i < datasUnprep[index].vertexStartIndex + datasUnprep[index].vertexCount; i++)
                {
                    vertsRdy[i] = Utils.TransformPoint(vertsUnprep[i], worldPos, worldAngle, loosyScale);
                }

                datasRdy[index] = data;
            }
            // EDGE
            else if (datasUnprep[index].typeEnum == ColliderType.Edge)
            {
                ColliderDataReady data = new()
                {
                    type = (int)ColliderType.Edge,
                    vertexStartIndex = datasUnprep[index].vertexStartIndex,
                    vertexCount = datasUnprep[index].vertexCount,
                    isClosed = datasUnprep[index].isClosedBool ? 1 : 0, // Edge is open.
                };
                
                for (int i = datasUnprep[index].vertexStartIndex;
                    i < datasUnprep[index].vertexStartIndex + datasUnprep[index].vertexCount; i++)
                {
                    vertsRdy[i] = Utils.TransformPoint(vertsUnprep[i], datasUnprep[index].posWorld,
                        datasUnprep[index].rotWorld, datasUnprep[index].lossyScale);
                }

                datasRdy[index] = data;
            }
            // COMPOSITE
            else if (datasUnprep[index].typeEnum == ColliderType.Composite)
            {
                // CompositeCollider2D may contain multiple paths. Add one ColliderData per path.
                // IMPORTANT: To avoid applying the scale twice, do not use TransformPoint here.
                ColliderDataReady data = new()
                {
                    type = (int)ColliderType.Composite,
                    vertexStartIndex = datasUnprep[index].vertexStartIndex,
                    vertexCount = datasUnprep[index].vertexCount,
                    // Assume composite shapes are closed.
                    isClosed = datasUnprep[index].isClosedBool ? 1 : 0,
                };
                
                Quaternion rot = Quaternion.Euler(0, 0, datasUnprep[index].rotWorld);

                for (int i = datasUnprep[index].vertexStartIndex;
                    i < datasUnprep[index].vertexStartIndex + datasUnprep[index].vertexCount; i++)
                {
                    vertsRdy[i] = (Vector2)datasUnprep[index].posWorld + 
                        (Vector2)(rot * vertsUnprep[i]);
                }
                
                datasRdy[index] = data;
            }
            // FALLBACK: use bounds as a box.
            else
            {
                ColliderDataReady data = new()
                {
                    type = (int)ColliderType.Box,
                    center = new float2(datasUnprep[index].posWorld.x, datasUnprep[index].posWorld.y),
                    size = new float2(datasUnprep[index].sizeLoc.x, datasUnprep[index].sizeLoc.y),
                };

                datasRdy[index] = data;
            }
        }
    }
}
