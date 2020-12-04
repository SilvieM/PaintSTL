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

    internal bool CheckPositionValid(DMesh3 mesh, Vector3d position, int colorToExclude)
    {
        var spatial = new DMeshAABBTree3(mesh);
        spatial.Build();
        
        spatial.TriangleFilterF = i => mesh.GetTriangleGroup(i) != colorToExclude;
        
        int near_tid = spatial.FindNearestTriangle(position, 9f);
        if (near_tid != DMesh3.InvalidID)
        {
            return false;
            //DistPoint3Triangle3 dist = MeshQueries.TriangleDistance(mesh, near_tid, position);
            //Vector3d nearest_pt = dist.TriangleClosest;
            //if (dist.DistanceSquared > 3) return true;
            //else return false;
        }

        return true;
    }

    //If the position is too close to shell, returns the direction to move away from it to try again. Returns null if not too close
    internal Vector3d? GetAwayFromShellDirection(DMeshAABBTree3 tree, Vector3d position, int colorToExclude)
    {

        tree.TriangleFilterF = i => tree.Mesh.GetTriangleGroup(i) != colorToExclude;

        int near_tid = tree.FindNearestTriangle(position, 3f);
        if (near_tid != DMesh3.InvalidID)
        {
            //var nearTri = mesh.GetTriangle(near_tid);
            return -tree.Mesh.GetTriNormal(near_tid);
        }

        return null;
    }

    internal Vector3d MoveUntilAwayFromShell(DMesh3 mesh, Vector3d position, int colorToExclude)
    {
        var tree = new DMeshAABBTree3(mesh, true);
        var getAway = this.GetAwayFromShellDirection(tree, position, colorToExclude);
        int count = 0;
        while (getAway != null)
        {
            position += getAway.Value.Normalized * 0.4;
            Debug.Log($"Getaway. New Pos: {position} ");
            getAway = GetAwayFromShellDirection(tree, position, colorToExclude);
            count++;
            if (count >= 100)
            {
                Debug.Log("MoveAway could not find a suitable position, count 100 exceeded");
            }
        }
        return position;
    }


    internal bool CheckIntersection(DMesh3 mesh, Triangle3d triangle)
    {
        var spatial = new DMeshAABBTree3(mesh);
        spatial.Build();
        return spatial.TestIntersection(triangle);

    }

    public virtual DMesh3 Cut(DMesh3 mesh, int colorId, double depth)
    {
        return mesh;
    }
}