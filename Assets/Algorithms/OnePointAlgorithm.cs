using System.Collections.Generic;
using System.Linq;
using Assets;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;
using UnityEngine;

public class OnePointAlgorithm : Algorithm
{


    public override DMesh3 Cut(DMesh3 mesh)
    {
        var painted = FindPaintedTriangles(mesh);
        if (painted.Count <= 0) return mesh;

        painted.Reverse();
        var currentGid = ColorManager.Instance.currentColorId ?? -1;
        var toDelete = new List<int>();
        var newMesh = new DMesh3();
        newMesh.EnableTriangleGroups();
        newMesh.EnableVertexColors(new Vector3f(1, 1, 1));
        var normals = new List<Vector3d>();
        var vertices = new List<Vector3d>();
        var verticesInNewMesh = new Dictionary<Vector3d, int>();
        foreach (var paintedTriNum in painted)
        {
            var tri = mesh.GetTriangle(paintedTriNum);
            var normal = mesh.GetTriNormal(paintedTriNum);
            normals.Add(normal);
            var orgA = mesh.GetVertex(tri.a);
            vertices.Add(orgA);
            var orgB = mesh.GetVertex(tri.b);
            vertices.Add(orgB);
            var orgC = mesh.GetVertex(tri.c);
            vertices.Add(orgC);
            var intA = StaticFunctions.AppendIfNotExists(verticesInNewMesh, orgA, newMesh);
            var intB = StaticFunctions.AppendIfNotExists(verticesInNewMesh, orgB, newMesh);
            var intC = StaticFunctions.AppendIfNotExists(verticesInNewMesh, orgC, newMesh);

            var result = mesh.RemoveTriangle(paintedTriNum);
            if (result != MeshResult.Ok) Debug.Log($"Removing did not work, {paintedTriNum} {result}");
            else toDelete.Add(paintedTriNum);
            newMesh.AppendTriangle(intA, intB, intC, currentGid);
        }

        var avgNormal = normals.Average();
        var avgVertices = vertices.Average();
        var newPoint = avgVertices - avgNormal;
        var newPointId = newMesh.AppendVertex(newPoint);
        var newPointIdInOldMesh = mesh.AppendVertex(newPoint);


        var eids = mesh.BoundaryEdgeIndices().ToList();
        foreach (var openEdge in eids)
        {
            AddTriangle(mesh, openEdge, newPointIdInOldMesh, 0);
        }

        var eidsNewMesh = newMesh.BoundaryEdgeIndices().ToList();
        foreach (var openEdge in eidsNewMesh)
        {
            AddTriangle(newMesh, openEdge, newPointId, currentGid);
        }

        mesh.SetVertexColor(newPointIdInOldMesh, ColorManager.Instance.currentColor.toVector3f());

        newMesh.SetVertexColor(newPointId, ColorManager.Instance.currentColor.toVector3f());
        var newObj = StaticFunctions.SpawnNewObject(newMesh); //TODO originalMesh?

        return mesh;
    }

    private void AddTriangle(DMesh3 currentMesh, int openEdge, int centerPoint, int currentGid)
    {
        var edge = currentMesh.GetOrientedBoundaryEdgeV(openEdge);
        currentMesh.AppendTriangle(edge.b, edge.a, centerPoint, currentGid);
    }
}