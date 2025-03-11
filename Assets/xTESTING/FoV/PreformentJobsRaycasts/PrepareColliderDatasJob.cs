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
                ColliderDataReady data = new();
                data.type = (int)ColliderType.Capsule;

                // Compute world center.
                Vector2 worldCenter = (Vector2)datasUnprep[index].posWorld +
                    (Vector2)(Quaternion.Euler(0, 0, datasUnprep[index].rotWorld) * datasUnprep[index].offsetLoc);

                // Get lossy scale.
                float width = datasUnprep[index].sizeLoc.x * datasUnprep[index].lossyScale.x;
                float height = datasUnprep[index].sizeLoc.y * datasUnprep[index].lossyScale.y;

                // CapsuleCollider2D.direction: 0 = horizontal, 1 = vertical.
                if (datasUnprep[index].capsuleDirEnum == CapsuleDirection2D.Vertical)
                {
                    data.capsuleRadius = width * 0.5f;
                    float segment = math.max(0f, height * 0.5f - data.capsuleRadius);
                    // Use transform.up for vertical orientation.
                    ///Vector2 up = capsule.transform.up;
                    data.capsuleA = worldCenter + (Vector2)datasUnprep[index].capsuleTransUp * segment;
                    data.capsuleB = worldCenter - (Vector2)datasUnprep[index].capsuleTransUp * segment;
                }
                else // Horizontal
                {
                    data.capsuleRadius = height * 0.5f;
                    float segment = math.max(0f, width * 0.5f - data.capsuleRadius);
                    ///Vector2 right = datasUnprep[index].capsuleTransRight;
                    data.capsuleA = worldCenter + (Vector2)datasUnprep[index].capsuleTransRight * segment;
                    data.capsuleB = worldCenter - (Vector2)datasUnprep[index].capsuleTransRight * segment;
                }

                datasRdy[index] = data;
            }
            // POLYGON
            else if (datasUnprep[index].typeEnum == ColliderType.Polygon)
            {
                ColliderDataReady data = new();
                data.type = (int)ColliderType.Polygon;

                data.vertexStartIndex = datasUnprep[index].vertexStartIndex;

                // poly.points are in local space; transform them to world space.
                ///Vector2[] polyPoint = poly.points;

                Vector2 worldPos = datasUnprep[index].posWorld;
                float worldAngle = datasUnprep[index].rotWorld;
                Vector2 loosyScale = datasUnprep[index].lossyScale;
                ///data.posWorld = poly.transform.position;
                ///data.rotWorld = poly.transform.eulerAngles.z;
                ///data.lossyScale = poly.transform.lossyScale;

                for (int i = datasUnprep[index].vertexStartIndex;
                    i < datasUnprep[index].vertexStartIndex + datasUnprep[index].vertexCount; i++)
                {
                    ///Vector2 worldPt = poly.transform.TransformPoint(polyPoint[i]);
                    vertsRdy[i] = Utils.TransformPoint(vertsUnprep[i], worldPos, worldAngle, loosyScale);

                    ///vertsUnprep.Add(poly.points[i]);
                }
                data.vertexCount = datasUnprep[index].vertexCount;

                ///data.isClosed = 1; // PolygonCollider2D is a closed shape.
                data.isClosed = datasUnprep[index].isClosedBool ? 1 : 0;

                ///datasUnprep.Add(data);
                datasRdy[index] = data;
            }
            // EDGE
            else if (datasUnprep[index].typeEnum == ColliderType.Edge)
            {
                ColliderDataReady data = new();
                data.type = (int)ColliderType.Edge;

                data.vertexStartIndex = datasUnprep[index].vertexStartIndex;
                for (int i = datasUnprep[index].vertexStartIndex;
                    i < datasUnprep[index].vertexStartIndex + datasUnprep[index].vertexCount; i++)
                {
                    ///Vector2 worldPt = edge.transform.TransformPoint(edge.points[i]);
                    vertsRdy[i] = Utils.TransformPoint(vertsUnprep[i], datasUnprep[index].posWorld,
                        datasUnprep[index].rotWorld, datasUnprep[index].lossyScale);

                    //vertsUnprep.Add(edge.points[i]);
                }
                ///data.posWorld = edge.transform.position;
                ///data.rotWorld = edge.transform.eulerAngles.z;
                ///data.lossyScale = edge.transform.lossyScale;

                data.vertexCount = datasUnprep[index].vertexCount;

                data.isClosed = datasUnprep[index].isClosedBool ? 1 : 0; // Edge is open.
                ///data.isClosedBool = false;

                //datasUnprep.Add(data);
                datasRdy[index] = data;
            }
            // COMPOSITE
            else if (datasUnprep[index].typeEnum == ColliderType.Composite)
            {
                // CompositeCollider2D may contain multiple paths. Add one ColliderData per path.
                // IMPORTANT: To avoid applying the scale twice, do not use TransformPoint here.
                ///for (int p = 0; p < composite.pathCount; p++)
                ///{

                ColliderDataReady data = new();
                data.type = (int)ColliderType.Composite;

                ///int pointCount = composite.GetPathPointCount(p);
                ///Vector2[] path = new Vector2[pointCount];
                ///composite.GetPath(p, path);

                data.vertexStartIndex = datasUnprep[index].vertexStartIndex;
                // Instead of using TransformPoint (which applies scale), only apply rotation and translation.
                ///Vector2 compPos = composite.transform.position;
                ///float compRot = composite.transform.eulerAngles.z;
                Quaternion rot = Quaternion.Euler(0, 0, datasUnprep[index].rotWorld);
                ///data.posWorld = composite.transform.position;
                ///data.rotWorld = composite.transform.eulerAngles.z;

                for (int i = datasUnprep[index].vertexStartIndex;
                    i < datasUnprep[index].vertexStartIndex + datasUnprep[index].vertexCount; i++)
                {
                    vertsRdy[i] = (Vector2)datasUnprep[index].posWorld +
                        (Vector2)(rot * vertsUnprep[i]);
                    ///vertsUnprep.Add(path[i]);
                }
                data.vertexCount = datasUnprep[index].vertexCount;

                // Assume composite shapes are closed.
                data.isClosed = datasUnprep[index].isClosedBool ? 1 : 0;
                ///data.isClosedBool = ;

                //datasUnprep.Add(data);
                datasRdy[index] = data;
                ///}
            }
            // FALLBACK: use bounds as a box.
            else
            {
                ColliderDataReady data = new();
                data.type = (int)ColliderType.Box;

                data.center = new float2(datasUnprep[index].posWorld.x, datasUnprep[index].posWorld.y);
                ///data.posWorld = col.bounds.center;

                ///data.rotationRad = 0f;

                data.size = new float2(datasUnprep[index].sizeLoc.x, datasUnprep[index].sizeLoc.y);
                ///data.sizeLoc = col.bounds.size;

                ///data.radius = 0f;

                ///datasUnprep.Add(data);
                datasRdy[index] = data;
            }
        }
    }
}
