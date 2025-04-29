using System.Collections.Generic;
using UnityEngine;

namespace Game.Physics
{
    public static class ColliderDataConverter
    {
        public static void UpdateColliderUnprepareDatas(FieldOfViewSystemCollectionsCache collections)
        {
            collections.datasUnprep.Clear();
            collections.vertsUnprep.Clear();

            foreach (var pair in collections.collidersUnprepared)
            {
                Collider2D col = pair.Value.Item1;
                Transform colTrans = col.transform;
                switch (col)
                {
                    case BoxCollider2D box:
                        CreateBoxData(collections, pair, col, colTrans, box);
                        break;

                    case CircleCollider2D circle:
                        CreateCircleData(collections, pair, col, colTrans, circle);
                        break;

                    case CapsuleCollider2D capsule:
                        CreateCapsueData(collections, pair, col, colTrans, capsule);
                        break;

                    case PolygonCollider2D poly:
                        CreatePolygonData(collections, pair, col, colTrans, poly);
                        break;

                    case EdgeCollider2D edge:
                        CreateEdgeData(collections, pair, col, colTrans, edge);
                        break;

                    case CompositeCollider2D composite:
                        CreateCompositeData(collections, pair, col, colTrans, composite);
                        break;

                    default:
                        CreateBoxDataForUnsuported(collections, pair, col);
                        break;
                }
            }
        }

        private static void CreateBoxData(FieldOfViewSystemCollectionsCache collections,
            KeyValuePair<int, (Collider2D, int)> pair, Collider2D col, Transform colTrans,
            BoxCollider2D box)
        {
            ColliderDataUnprepared boxData = new()
            {
                typeEnum = ColliderType.Box,
                posWorld = (Vector2)colTrans.position,
                rotWorld = colTrans.eulerAngles.z,
                offsetLoc = box.offset,
                lossyScale = (Vector2)colTrans.lossyScale,
                sizeLoc = box.size,
                colliderId = pair.Key,
                layer = col.gameObject.layer
            };
            collections.datasUnprep.Add(boxData);
        }

        private static void CreateCircleData(FieldOfViewSystemCollectionsCache collections,
            KeyValuePair<int, (Collider2D, int)> pair, Collider2D col, Transform colTrans,
            CircleCollider2D circle)
        {
            ColliderDataUnprepared circleData = new()
            {
                typeEnum = ColliderType.Circle,
                posWorld = (Vector2)colTrans.position,
                rotWorld = colTrans.eulerAngles.z,
                offsetLoc = circle.offset,
                lossyScale = (Vector2)colTrans.localScale,
                radiusLoc = circle.radius,
                colliderId = pair.Key,
                layer = col.gameObject.layer
            };
            collections.datasUnprep.Add(circleData);
        }

        private static void CreateCapsueData(FieldOfViewSystemCollectionsCache collections,
            KeyValuePair<int, (Collider2D, int)> pair, Collider2D col, Transform colTrans,
            CapsuleCollider2D capsule)
        {
            ColliderDataUnprepared capsuleData = new()
            {
                typeEnum = ColliderType.Capsule,
                offsetLoc = capsule.offset,
                posWorld = (Vector2)colTrans.position,
                rotWorld = colTrans.eulerAngles.z,
                lossyScale = (Vector2)colTrans.lossyScale,
                sizeLoc = capsule.size,
                capsuleDirEnum = capsule.direction,
                capsuleTransUpOrBoundsPos = (Vector2)colTrans.up,
                capsuleTransRightOrBoundsSize = (Vector2)colTrans.right,
                colliderId = pair.Key,
                layer = col.gameObject.layer
            };
            collections.datasUnprep.Add(capsuleData);
        }

        private static void CreatePolygonData(FieldOfViewSystemCollectionsCache collections,
            KeyValuePair<int, (Collider2D, int)> pair, Collider2D col, Transform colTrans,
            PolygonCollider2D poly)
        {
            Vector2[] points = poly.points;

            Bounds boundsPoly = col.bounds;
            ColliderDataUnprepared polyData = new()
            {
                typeEnum = ColliderType.Polygon,
                vertexStartIndex = collections.vertsUnprep.Length,
                posWorld = (Vector2)colTrans.position,
                rotWorld = colTrans.eulerAngles.z,
                lossyScale = (Vector2)colTrans.lossyScale,
                vertexCount = points.Length,
                capsuleTransUpOrBoundsPos = (Vector2)boundsPoly.center,
                capsuleTransRightOrBoundsSize = (Vector2)boundsPoly.size,
                colliderId = pair.Key,
                layer = col.gameObject.layer
            };

            unsafe
            {
                fixed (Vector2* ptr = points)
                {
                    collections.vertsUnprep.AddRange(ptr, points.Length);
                }
            }

            collections.datasUnprep.Add(polyData);
        }

        private static void CreateEdgeData(FieldOfViewSystemCollectionsCache collections,
            KeyValuePair<int, (Collider2D, int)> pair, Collider2D col, Transform colTrans,
            EdgeCollider2D edge)
        {
            Vector2[] edgePoints = edge.points;
            Bounds boundsEdge = col.bounds;
            ColliderDataUnprepared edgeData = new()
            {
                typeEnum = ColliderType.Edge,
                vertexStartIndex = collections.vertsUnprep.Length,
                posWorld = (Vector2)colTrans.position,
                rotWorld = colTrans.eulerAngles.z,
                lossyScale = (Vector2)colTrans.lossyScale,
                vertexCount = edgePoints.Length,
                capsuleTransUpOrBoundsPos = (Vector2)boundsEdge.center,
                capsuleTransRightOrBoundsSize = (Vector2)boundsEdge.size,
                colliderId = pair.Key,
                layer = col.gameObject.layer
            };

            unsafe
            {
                fixed (Vector2* ptr = edgePoints)
                {
                    collections.vertsUnprep.AddRange(ptr, edgePoints.Length);
                }
            }

            collections.datasUnprep.Add(edgeData);
        }

        private static void CreateCompositeData(FieldOfViewSystemCollectionsCache collections,
            KeyValuePair<int, (Collider2D, int)> pair, Collider2D col, Transform colTrans,
            CompositeCollider2D composite)
        {
            for (int p = 0; p < composite.pathCount; p++)
            {
                int pointCount = composite.GetPathPointCount(p);

                Bounds boundsComposite = col.bounds;
                ColliderDataUnprepared compositeData = new()
                {
                    typeEnum = ColliderType.Composite,
                    vertexStartIndex = collections.vertsUnprep.Length,
                    posWorld = (Vector2)colTrans.position,
                    rotWorld = colTrans.eulerAngles.z,
                    vertexCount = pointCount,
                    capsuleTransUpOrBoundsPos = (Vector2)boundsComposite.center,
                    capsuleTransRightOrBoundsSize = (Vector2)boundsComposite.size,
                    colliderId = pair.Key,
                    layer = col.gameObject.layer
                };

                unsafe
                {
                    if (collections.pathPointsCompositeCache.Length < pointCount)
                    {
                        collections.pathPointsCompositeCache = new Vector2[pointCount];
                    }
                    composite.GetPath(p, collections.pathPointsCompositeCache);
                    fixed (Vector2* pathPointsPtr = collections.pathPointsCompositeCache)
                    {
                        collections.vertsUnprep.AddRange(pathPointsPtr, pointCount);
                    }
                }

                collections.datasUnprep.Add(compositeData);
            }
        }

        private static void CreateBoxDataForUnsuported(FieldOfViewSystemCollectionsCache collections,
            KeyValuePair<int, (Collider2D, int)> pair, Collider2D col)
        {
            ColliderDataUnprepared defaultData = new()
            {
                typeEnum = ColliderType.Unsuported,
                posWorld = (Vector2)col.bounds.center,
                sizeLoc = (Vector2)col.bounds.size,
                colliderId = pair.Key,
                layer = col.gameObject.layer
            };
            collections.datasUnprep.Add(defaultData);
            Debug.LogError("Unsuported collider type");
        }
    }
}
