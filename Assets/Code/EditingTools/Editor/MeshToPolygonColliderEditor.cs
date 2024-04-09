using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class SetPolygonCollider3D
{
    [MenuItem("SpaceWars/Update Polygon Collider %t", false, -1)]
    static void UpdatePolygonColliders()
    {
        Transform transform = Selection.activeTransform;

        if (transform == null)
        {
            Debug.LogWarning("No valid GameObject selected!");
            return;
        }

        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogError("There is no GameObject selected. Returning...");
            return;
        }

        if(Selection.gameObjects.Length > 2)
        {
            Debug.LogError("There are more than 2 GameObjects selected. Returning...");
            return;
        }

        MeshFilter selectedMeshFilter = null;
        PolygonCollider2D selectedCollider = null;
        foreach (var selectedObject in Selection.gameObjects)
        {
            MeshFilter meshFilter = selectedObject.GetComponent<MeshFilter>();
            PolygonCollider2D collider = selectedObject.GetComponent<PolygonCollider2D>();

            if(meshFilter != null)
            {
                if(selectedMeshFilter != null)
                {
                    Debug.LogError("There are more than one GameObjects with MeshFilter. Retruning...");
                    return;
                }
                selectedMeshFilter = meshFilter;
            }

            if(collider != null)
            {
                if(selectedCollider != null)
                {
                    Debug.LogError("There are more than one GameObjects with PolygonCollider2D. Retruning...");
                    return;
                }
                selectedCollider = collider;
            }
        }

        if(selectedMeshFilter == null)
        {
            Debug.LogError("There isn't any selected MeshFilter. Returning...");
            return;
        }

        if(selectedCollider == null)
        {
            if (Selection.gameObjects.Length == 1)
            {
                selectedCollider = Selection.gameObjects[0].AddComponent<PolygonCollider2D>();
            }
            else
            {
                foreach (var selectedObject in Selection.gameObjects)
                {
                    if (selectedObject == selectedMeshFilter.gameObject)
                        continue;

                    selectedCollider = selectedObject.AddComponent<PolygonCollider2D>();
                }
            }
        }

        if (selectedCollider == null)
        {
            Debug.LogError("There isn't any selected MeshFilter. Returning...");
            return;
        }

        UpdatePolygonCollider2D(selectedMeshFilter,selectedCollider);

        Selection.SetActiveObjectWithContext(selectedCollider, null);
    }

    static void UpdatePolygonCollider2D(MeshFilter meshFilter, PolygonCollider2D collider)
    {
        if (meshFilter.sharedMesh == null)
        {
            Debug.LogWarning(meshFilter.gameObject.name + " has no Mesh set on its MeshFilter component!");
            return;
        }

        collider.pathCount = 1;

        List<Vector3> vertices = new List<Vector3>();
        meshFilter.sharedMesh.GetVertices(vertices);

        vertices = EdgeHelpers.RotateAndScaleVertices(meshFilter,
            meshFilter.transform.localRotation.eulerAngles, meshFilter.transform.localScale);

        var boundaryPath = EdgeHelpersBase.GetEdges(meshFilter.sharedMesh.triangles)
            .FindBoundary().SortEdges();

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
        EditorUtility.SetDirty(collider);
        collider.SetPath(0, newPoints);

        // Make the collider convex
        collider.points = MakeConvex(collider.points);
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

    public static List<Vector3> RotateAndScaleVertices(MeshFilter meshFilter
        , Vector3 rotationAngles, Vector3 scaleFactors)
    {
        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null)
        {
            Debug.LogError("Mesh not found!");
            return null;
        }

        // Clone the original vertices
        Vector3[] originalVertices = mesh.vertices;
        List<Vector3> rotatedVertices = new List<Vector3>(originalVertices.Length);

        // Apply rotation and scale
        Quaternion rotation = Quaternion.Euler(rotationAngles);
        for (int i = 0; i < originalVertices.Length; i++)
        {
            // Rotate and scale each vertex
            Vector3 rotatedVertex = rotation * originalVertices[i];
            rotatedVertex = Vector3.Scale(rotatedVertex, scaleFactors);
            rotatedVertices.Add(rotatedVertex);
        }

        return rotatedVertices;
    }
}
