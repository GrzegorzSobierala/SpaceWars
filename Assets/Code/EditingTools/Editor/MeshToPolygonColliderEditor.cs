using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.SceneManagement;
using System.Linq;

public static class SetPolygonCollider3D
{
    [MenuItem("Tools/Update Polygon Colliders %t", false, -1)]
    static void UpdatePolygonColliders()
    {
        Transform transform = Selection.activeTransform;
        if (transform == null)
        {
            Debug.LogWarning("No valid GameObject selected!");
            return;
        }

        EditorSceneManager.MarkSceneDirty(transform.gameObject.scene);

        MeshFilter[] meshFilters = transform.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            PolygonCollider2D polygonCollider2D;
            if (!meshFilter.TryGetComponent(out polygonCollider2D))
            {
                meshFilter.gameObject.AddComponent<PolygonCollider2D>();
            }
            
            UpdatePolygonCollider2D(meshFilter);
        }
    }

    static void UpdatePolygonCollider2D(MeshFilter meshFilter)
    {
        if (meshFilter.sharedMesh == null)
        {
            Debug.LogWarning(meshFilter.gameObject.name + " has no Mesh set on its MeshFilter component!");
            return;
        }

        PolygonCollider2D polygonCollider2D = meshFilter.GetComponent<PolygonCollider2D>();
        polygonCollider2D.pathCount = 1;

        List<Vector3> vertices = new List<Vector3>();
        meshFilter.sharedMesh.GetVertices(vertices);


        int[] targetTriangles = EdgeHelpersBase.RotateAndScaleTriangles(meshFilter,
            meshFilter.transform.localRotation.eulerAngles, meshFilter.transform.localScale);

        var boundaryPath = EdgeHelpersBase.GetEdges(targetTriangles).FindBoundary().SortEdges();

        Vector3[] yourVectors = new Vector3[boundaryPath.Count];
        for (int i = 0; i < boundaryPath.Count; i++)
        {
            yourVectors[i] = vertices[boundaryPath[i].v1];
        }
        List<Vector2> newColliderVertices = new List<Vector2>();

        for (int i = 0; i < yourVectors.Length; i++)
        {
            newColliderVertices.Add(new Vector2(yourVectors[i].x, yourVectors[i].y));
        }

        Vector2[] newPoints = newColliderVertices.Distinct().ToArray();

        // Set the new points for the PolygonCollider2D
        EditorUtility.SetDirty(polygonCollider2D);
        polygonCollider2D.SetPath(0, newPoints);

        // Make the collider convex
        polygonCollider2D.points = MakeConvex(polygonCollider2D.points);
        Debug.Log(meshFilter.gameObject.name + " PolygonCollider2D updated and made convex.");
    }

    static Vector2[] MakeConvex(Vector2[] points)
    {
        List<Vector2> convexPoints = new List<Vector2>(points);

        // Perform convex hull computation
        convexPoints = ConvexHull(convexPoints);

        return convexPoints.ToArray();
    }

    static List<Vector2> ConvexHull(List<Vector2> points)
    {
        // Sort points by x-coordinate
        points.Sort((a, b) => a.x.CompareTo(b.x));

        // Initialize lists for upper and lower hulls
        List<Vector2> upperHull = new List<Vector2>();
        List<Vector2> lowerHull = new List<Vector2>();

        // Build upper hull
        foreach (var point in points)
        {
            while (upperHull.Count >= 2 && IsClockwiseTurn(upperHull[upperHull.Count - 2], upperHull[upperHull.Count - 1], point))
            {
                upperHull.RemoveAt(upperHull.Count - 1);
            }
            upperHull.Add(point);
        }

        // Build lower hull
        for (int i = points.Count - 1; i >= 0; i--)
        {
            while (lowerHull.Count >= 2 && IsClockwiseTurn(lowerHull[lowerHull.Count - 2], lowerHull[lowerHull.Count - 1], points[i]))
            {
                lowerHull.RemoveAt(lowerHull.Count - 1);
            }
            lowerHull.Add(points[i]);
        }

        // Remove the first and last points (they are the same in both hulls)
        upperHull.RemoveAt(upperHull.Count - 1);
        lowerHull.RemoveAt(lowerHull.Count - 1);

        // Concatenate upper and lower hulls to form the convex hull
        upperHull.AddRange(lowerHull);

        return upperHull;
    }

    static bool IsClockwiseTurn(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x) < 0;
    }
}

public static class EdgeHelpersBase
{
    public struct Edge
    {
        public int v1;
        public int v2;
        public int triangleIndex;
        public Edge(int aV1, int aV2, int aIndex)
        {
            v1 = aV1;
            v2 = aV2;
            triangleIndex = aIndex;
        }
    }

    public static List<Edge> GetEdges(int[] aIndices)
    {
        List<Edge> result = new List<Edge>();
        for (int i = 0; i < aIndices.Length; i += 3)
        {
            int v1 = aIndices[i];
            int v2 = aIndices[i + 1];
            int v3 = aIndices[i + 2];
            result.Add(new Edge(v1, v2, i));
            result.Add(new Edge(v2, v3, i));
            result.Add(new Edge(v3, v1, i));
        }
        return result;
    }

    public static List<Edge> FindBoundary(this List<Edge> aEdges)
    {
        List<Edge> result = new List<Edge>(aEdges);
        for (int i = result.Count - 1; i > 0; i--)
        {
            for (int n = i - 1; n >= 0; n--)
            {
                if (result[i].v1 == result[n].v2 && result[i].v2 == result[n].v1)
                {
                    // shared edge so remove both
                    result.RemoveAt(i);
                    result.RemoveAt(n);
                    i--;
                    break;
                }
            }
        }
        return result;
    }
    public static List<Edge> SortEdges(this List<Edge> aEdges)
    {
        List<Edge> result = new List<Edge>(aEdges);
        for (int i = 0; i < result.Count - 2; i++)
        {
            Edge E = result[i];
            for (int n = i + 1; n < result.Count; n++)
            {
                Edge a = result[n];
                if (E.v2 == a.v1)
                {
                    // in this case they are already in order so just continue with the next one
                    if (n == i + 1)
                        break;
                    // if we found a match, swap them with the next one after "i"
                    result[n] = result[i + 1];
                    result[i + 1] = a;
                    break;
                }
            }
        }
        return result;
    }


    public static int[] RotateAndScaleTriangles(MeshFilter meshFilter, Vector3 rotationAngles, 
        Vector3 scaleFactors)
    {
        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null)
        {
            Debug.LogError("Mesh not found!");
            return null;
        }

        // Clone the original triangles
        int[] originalTriangles = mesh.triangles;
        int[] rotatedTriangles = new int[originalTriangles.Length];
        originalTriangles.CopyTo(rotatedTriangles, 0);

        // Apply rotation and scale
        Quaternion rotation = Quaternion.Euler(rotationAngles);
        for (int i = 0; i < rotatedTriangles.Length; i++)
        {
            // Rotate vertex positions
            Vector3 vertexPosition = mesh.vertices[rotatedTriangles[i]];
            vertexPosition = rotation * vertexPosition;
            vertexPosition = Vector3.Scale(vertexPosition, scaleFactors);

            // Find the closest vertex to the rotated position
            float minDistance = float.MaxValue;
            int closestVertexIndex = -1;
            for (int j = 0; j < mesh.vertices.Length; j++)
            {
                float distance = Vector3.Distance(vertexPosition, mesh.vertices[j]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestVertexIndex = j;
                }
            }

            // Update the index to the closest vertex
            rotatedTriangles[i] = closestVertexIndex;
        }

        return rotatedTriangles;
    }
}
