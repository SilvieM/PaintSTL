using System.Collections.Generic;
using System.Linq;
using Assets;
using Assets.Algorithms;
using Assets.Classes;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;
using UnityEngine;

public class OnePointAlgorithm : Algorithm
{


    public override DMesh3 Cut(CuttingInfo info)
    {
        var painted = FindPaintedTriangles(info.mesh, info.data.ColorNum);
        if (painted.Count <= 0) return info.mesh;

        painted.Reverse();
        var newMesh = new DMesh3();
        newMesh.EnableTriangleGroups();
        newMesh.EnableVertexColors(new Vector3f(1, 1, 1));
        var normals = new List<Vector3d>();
        var vertices = new List<Vector3d>();
        var verticesInNewMesh = new Dictionary<Vector3d, int>();
        foreach (var paintedTriNum in painted)
        {
            var tri = info.mesh.GetTriangle(paintedTriNum);
            var normal = info.mesh.GetTriNormal(paintedTriNum);
            normals.Add(normal);
            var orgA = info.mesh.GetVertex(tri.a);
            vertices.Add(orgA);
            var orgB = info.mesh.GetVertex(tri.b);
            vertices.Add(orgB);
            var orgC = info.mesh.GetVertex(tri.c);
            vertices.Add(orgC);
            var intA = StaticFunctions.AppendIfNotExists(verticesInNewMesh, orgA, newMesh);
            var intB = StaticFunctions.AppendIfNotExists(verticesInNewMesh, orgB, newMesh);
            var intC = StaticFunctions.AppendIfNotExists(verticesInNewMesh, orgC, newMesh);

            var result = info.mesh.RemoveTriangle(paintedTriNum);
            if (result != MeshResult.Ok) Debug.Log($"Removing did not work, {paintedTriNum} {result}");
            newMesh.AppendTriangle(intA, intB, intC, info.data.ColorNum);
        }

        var avgNormal = normals.Average();
        var avgVertices = vertices.Average();
        var newPoint = avgVertices - avgNormal* info.data.depth;

        if(info.data.modifier == CutSettingData.Modifier.Compute) newPoint = MovePointInsideAndAwayFromShell(info, newPoint);
        if (info.data.modifier == CutSettingData.Modifier.DepthDependant) newPoint = MovePointDepthDependant(info, avgVertices, avgNormal);
        


        var newPointId = newMesh.AppendVertex(newPoint);
        var newPointIdInOldMesh = info.mesh.AppendVertex(newPoint);


        var eids = info.mesh.BoundaryEdgeIndices().ToList();
        foreach (var openEdge in eids)
        {
            AddTriangle(info.mesh, openEdge, newPointIdInOldMesh, 0);
        }

        var eidsNewMesh = newMesh.BoundaryEdgeIndices().ToList();
        foreach (var openEdge in eidsNewMesh)
        {
            AddTriangle(newMesh, openEdge, newPointId, info.data.ColorNum);
        }

        info.mesh.SetVertexColor(newPointIdInOldMesh, ColorManager.Instance.GetColorForId(info.data.ColorNum).toVector3f());

        newMesh.SetVertexColor(newPointId, ColorManager.Instance.GetColorForId(info.data.ColorNum).toVector3f());
        var newObj = StaticFunctions.SpawnNewObject(newMesh);

        return info.mesh;
    }

    private void AddTriangle(DMesh3 currentMesh, int openEdge, int centerPoint, int currentGid)
    {
        var edge = currentMesh.GetOrientedBoundaryEdgeV(openEdge);
        //var triangle = new Triangle3d(currentMesh.GetVertex(edge.b), currentMesh.GetVertex(edge.a), currentMesh.GetVertex(centerPoint));
        //if(this.CheckIntersection(currentMesh, triangle)) Debug.Log("Intersection found");
        currentMesh.AppendTriangle(edge.b, edge.a, centerPoint, currentGid);
    }
}