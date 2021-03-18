using System.Collections.Generic;
using Assets;
using Assets.Algorithms;
using Assets.Static_Classes;
using g3;
using UnityEngine;

public class Algorithm
{
    public enum AlgorithmType {Backside, OnePoint, HoleFill, OffsetHoleFill, Ignore }

    public static Algorithm BuildAlgo(AlgorithmType whichAlgo)
    {
        switch (whichAlgo)
        {
            case AlgorithmType.Ignore: return null;
            case AlgorithmType.OnePoint: return new OnePointAlgorithm();
            case AlgorithmType.Backside: return new BacksideAlgorithm();
            case AlgorithmType.HoleFill: return new HoleFillAlgorithm();
            case AlgorithmType.OffsetHoleFill: return new OffsetHoleFill();
        }

        return null;
    }

    public List<int> FindPaintedTriangles(DMesh3 mesh, int colorId)
    {
        List<int> indices = FaceGroupUtil.FindTrianglesByGroup(mesh, colorId);
        Debug.Log($"Painted Triangles: {indices.Count}");

        if (indices.Count <= 0)
        {
            Debug.Log("No colored triangles found");
        }
        return indices;
    }

    public static MeshConnectedComponents FindConnectedComponents(CuttingInfo info, List<int> painted)
    {
        var components = new MeshConnectedComponents(info.mesh);
        if (info.data.Multipiece)
        {
            components.FilterF = i => info.mesh.GetTriangleGroup(i) == info.data.ColorNum;
            components.FindConnectedT();
        }
        else
        {
            var newC = new MeshConnectedComponents.Component
            {
                Indices = painted.ToArray()
            };
            components.Components.Add(newC);
        }

        return components;
    }

    public static void InstantiateNewObjects(CuttingInfo info, List<DMesh3> subMeshes)
    {
        if (info.data.Multipiece||subMeshes.Count<=1)
        {
            foreach (var subMesh in subMeshes)
            {
                subMesh.EnableTriangleGroups(info.data.ColorNum);
                var newObj = StaticFunctions.SpawnNewObject(subMesh);
                newObj.GetComponent<Generate>().cuttingInfo = info;
            }
        }
        else
        {
            var totalNewMesh = MeshEditor.Combine(subMeshes.ToArray());
            totalNewMesh.EnableTriangleGroups(info.data.ColorNum);
            var newObj = StaticFunctions.SpawnNewObject(totalNewMesh);
            newObj.GetComponent<Generate>().cuttingInfo = info; //todo we need to fix PointToPoint for Point algo if we combine
        }
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
        }

        return true;
    }

    //If the position is too close to shell, returns the direction to move away from it to try again. Returns null if not too close
    internal Vector3d? GetAwayFromShellDirection(CuttingInfo info, DMeshAABBTree3 tree, Vector3d position)
    {

        tree.TriangleFilterF = i => tree.Mesh.GetTriangleGroup(i) != info.data.ColorNum;

        int near_tid = tree.FindNearestTriangle(position, info.data.minDepth); //TODO scale the max dist by SDF or so
        if (near_tid != DMesh3.InvalidID)
        {
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

    internal Vector3d MovePointInsideAndAwayFromShell(CuttingInfo info, Vector3d position, int maxCount = 5)
    {
        var tree = new DMeshAABBTree3(info.mesh, true);
        if (!tree.IsInside(position))
        {
            Debug.Log("Point outside of mesh");
            var getInside = GetInsideShell(tree, position, info.data.ColorNum);
            if (getInside != null){
                position += getInside.Value;
            Debug.Log($"GetInside. New Pos: {position} ");
            }
            else
            {
                Debug.Log("Point too far away from shell");
            }

        }
        var getAway = this.GetAwayFromShellDirection(info, tree, position);
        int count = 0;
        while (getAway != null)
        {
            position += getAway.Value.Normalized * info.data.minDepth; 
            Debug.Log($"Getaway. New Pos: {position} ");

            count++;

            if (count >= maxCount)
            {
                Debug.Log("MoveAway could not find a suitable position, count exceeded");
                StaticFunctions.ErrorMessage("The object is too thin to find a suitable position. Might cause intersections.");
                break;
            }
            getAway = GetAwayFromShellDirection(info, tree, position);
        }
        return position;
    }

    internal void MoveAllPointsDepthDependant(CuttingInfo info, DMesh3 newMesh, Dictionary<int, BacksideAlgorithm.PeprStatusVert> stati)
    {
        var tree = new DMeshAABBTree3(info.oldMesh, true);
        tree.TriangleFilterF = i => tree.Mesh.GetTriangleGroup(i) != info.data.ColorNum; 
        foreach (var status in stati)
        {
            var shellPoint = newMesh.GetVertex(status.Value.idNewMeshOuter.Value);
            var normal = info.oldMesh.CalcVertexNormal(status.Value.idOldMeshOuter);
            var position = shellPoint + info.data.minDepth * normal;
            Ray3d ray = new Ray3d(shellPoint-normal*info.data.minDepth, -normal); //tiny shift to make sure it's not hitting itself
            int hit_tid = tree.FindNearestHitTriangle(ray);
            Debug.Log("Hit " + hit_tid);
            if (hit_tid != DMesh3.InvalidID)
            {
                IntrRay3Triangle3 intr = MeshQueries.TriangleIntersection(info.oldMesh, hit_tid, ray);
                double hit_dist = shellPoint.Distance(ray.PointAt(intr.RayParameter));
                position = shellPoint - normal * hit_dist * (info.data.depth / 100);
                Debug.Log($"Hit Dist: {hit_dist}");
            }
            else
            {
                StaticFunctions.ErrorMessage("Depth Dependant Calculation has encountered an error");
            }
            info.mesh.SetVertex(status.Value.idOldMeshInner.Value, position);
            newMesh.SetVertex(status.Value.idNewMeshInner.Value, position);
        }
    }

    internal Vector3d MovePointDepthDependant(CuttingInfo info, Vector3d shellPoint, Vector3d normal)
    {
        normal = normal.Normalized;
        var position = shellPoint + info.data.minDepth * normal; ;
        var tree = new DMeshAABBTree3(info.oldMesh, true);
        Ray3d ray = new Ray3d(shellPoint, -normal);
        int hit_tid = tree.FindNearestHitTriangle(ray);
        Debug.Log("Hit " + hit_tid);
        if (hit_tid != DMesh3.InvalidID)
        {
            IntrRay3Triangle3 intr = MeshQueries.TriangleIntersection(info.oldMesh, hit_tid, ray);
            double hit_dist = shellPoint.Distance(ray.PointAt(intr.RayParameter));
            position = shellPoint-normal * hit_dist * (info.data.depth / 100);
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