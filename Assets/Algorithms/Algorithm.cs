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
            var howFar = 0.1;
            Ray3d ray = new Ray3d(position, getAway.Value.Normalized);
            int hit_tid = tree.FindNearestHitTriangle(ray);
            Debug.Log("Hit "+hit_tid);
            if (hit_tid != DMesh3.InvalidID)
            {
                IntrRay3Triangle3 intr = MeshQueries.TriangleIntersection(info.oldMesh, hit_tid, ray);
                double hit_dist = position.Distance(ray.PointAt(intr.RayParameter));
                howFar = hit_dist * 0.2; //going 1/5 the way we can go
                Debug.Log($"How far: {howFar}");
            }

            position += getAway.Value.Normalized * howFar; 
            Debug.Log($"Getaway. New Pos: {position} ");
            count++;

            if (count >= 5)
            {
                Debug.Log("MoveAway could not find a suitable position, count exceeded");
                StaticFunctions.ErrorMessage("The object is too thin to find a suitable position. Might cause intersections.");
                break;
            }
            getAway = GetAwayFromShellDirection(tree, position, info.colorId);
        }
        return position;
    }

    internal Vector3d MovePointDepthDependant(CuttingInfo info, Vector3d shellPoint, Vector3d normal)
    {
        normal = normal.Normalized;
        var position = shellPoint;
        var tree = new DMeshAABBTree3(info.oldMesh, true);
        Ray3d ray = new Ray3d(shellPoint, -normal);
        int hit_tid = tree.FindNearestHitTriangle(ray);
        Debug.Log("Hit " + hit_tid);
        if (hit_tid != DMesh3.InvalidID)
        {
            IntrRay3Triangle3 intr = MeshQueries.TriangleIntersection(info.oldMesh, hit_tid, ray);
            double hit_dist = shellPoint.Distance(ray.PointAt(intr.RayParameter));
            position = shellPoint-normal * hit_dist * (info.depth / 100);
            Debug.Log($"Hit Dist: {hit_dist}");
        }
        else
        {
            StaticFunctions.ErrorMessage("Depth Dependant Calculation failed");
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