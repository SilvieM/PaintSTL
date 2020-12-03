using System.Collections.Generic;
using Assets;
using g3;
using UnityEngine;

public class Algorithm
{
    public enum AlgorithmType {OnePoint, Pepr}

    public static Algorithm BuildAlgo(AlgorithmType whichAlgo)
    {
        switch (whichAlgo)
        {
            case AlgorithmType.OnePoint: return new OnePointAlgorithm();
            case AlgorithmType.Pepr: return new PeprAlgorithm();
        }

        return null;
    }

    public List<int> FindPaintedTriangles(DMesh3 mesh, int colorId)
    {
        List<int> indices = new List<int>();
        foreach (var triangleIndex in mesh.TriangleIndices())
        {
                if (mesh.GetTriangleGroup(triangleIndex) == colorId)
                    indices.Add(triangleIndex);
        }

        Debug.Log($"Painted Triangles: {indices.Count}");

        if (indices.Count <= 0)
        {
            Debug.Log("No colored triangles found");
        }
        return indices;
    }

    public virtual DMesh3 Cut(DMesh3 mesh, int colorId)
    {
        return mesh;
    }
}