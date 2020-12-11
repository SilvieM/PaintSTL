using System.Collections.Generic;
using Assets;
using Assets.Algorithms;
using Assets.Static_Classes;
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

        int near_tid = tree.FindNearestTriangle(position, 3f); //TODO scale the max dist by SDF or so
        if (near_tid != DMesh3.InvalidID)
        {
            //var nearTri = mesh.
            //GetTriangle(near_tid);
            return -tree.Mesh.GetTriNormal(near_tid);
        }

        return null;
    }

    internal Vector3d? GetInsideShell(DMeshAABBTree3 tree, Vector3d position, int colorToExclude)
    {

        tree.TriangleFilterF = i => tree.Mesh.GetTriangleGroup(i) != colorToExclude;

        int near_tid = tree.FindNearestTriangle(position, 30f);
        if (near_tid != DMesh3.InvalidID)
        {
            //var nearTri = mesh.GetTriangle(near_tid);
            return tree.Mesh.GetTriCentroid(near_tid)-position;
        }
        else
        {
            Debug.Log("Get Inside: Too far away");
            StaticFunctions.ErrorMessage("Extrusion/Depth setting of a color is too much, might extrude outside of shell");
        }

        return null;
    }

    internal Vector3d MovePointInsideAndAwayFromShell(CuttingInfo info, Vector3d position)
    {
        var tree = new DMeshAABBTree3(info.oldMesh, true);
        if (!tree.IsInside(position))
        {
            Debug.Log("Point outside of mesh");
            var getInside = GetInsideShell(tree, position, info.colorId);
            if (getInside != null){
                position += getInside.Value;
            Debug.Log($"GetInside. New Pos: {position} ");
            }
            else
            {
                Debug.Log("Point too far away from shell");
            }

        }
        var getAway = this.GetAwayFromShellDirection(tree, position, info.colorId);
        int count = 0;
        while (getAway != null)
        {
            position += getAway.Value.Normalized * 0.4; //TODO scale this with SDF??
            Debug.Log($"Getaway. New Pos: {position} ");
            getAway = GetAwayFromShellDirection(tree, position, info.colorId);
            count++;
            if (count >= 10)
            {
                Debug.Log("MoveAway could not find a suitable position, count exceeded");
                StaticFunctions.ErrorMessage("The object is too thin to find a suitable position. Might cause intersections.");
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

    public virtual DMesh3 Cut(CuttingInfo info)
    {
        return info.mesh;
    }
}