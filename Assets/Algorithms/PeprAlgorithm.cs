using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;
using UnityEngine;

public class PeprAlgorithm : Algorithm
{
    public override DMesh3 Cut(DMesh3 mesh)
    {
        var painted = FindPaintedTriangles(mesh);
        if (painted.Count <= 0) return mesh;
        var currentGid = ColorManager.Instance.currentColorId ?? -1;
        var newMesh = new DMesh3();
        newMesh.EnableTriangleGroups();
        newMesh.EnableVertexColors(new Vector3f(1, 1, 1));

        var verticesInNewMesh = new Dictionary<Vector3d, int>();
        var verticesInOldMesh = new Dictionary<Vector3d, int>();
        var InnerToOuter = new Dictionary<int, int>();
        var InnerToOuterOldMesh = new Dictionary<int, int>();
        foreach (var triIndex in painted)
        {
            var triangle = mesh.GetTriangle(triIndex);
            var vertex1 = mesh.GetVertex(triangle.a);
            var vertex2 = mesh.GetVertex(triangle.b);
            var vertex3 = mesh.GetVertex(triangle.c);

            var intAOuter = StaticFunctions.AppendIfNotExists(verticesInNewMesh, vertex1, newMesh);
            var intBOuter = StaticFunctions.AppendIfNotExists(verticesInNewMesh, vertex2, newMesh); 
            var intCOuter = StaticFunctions.AppendIfNotExists(verticesInNewMesh, vertex3, newMesh);

            var newTriOuter = newMesh.AppendTriangle(intAOuter, intBOuter, intCOuter, currentGid);

            var normal = mesh.GetTriNormal(triIndex);
            var normal1 = mesh.CalcVertexNormal(triangle.a);
            var normal2 = mesh.CalcVertexNormal(triangle.b);
            var normal3 = mesh.CalcVertexNormal(triangle.c);

            var intAInner = StaticFunctions.AppendIfNotExists(verticesInNewMesh, vertex1 - normal1*4, newMesh);
            var intBInner = StaticFunctions.AppendIfNotExists(verticesInNewMesh, vertex3 - normal3*4, newMesh); //swapping to mirror
            var intCInner = StaticFunctions.AppendIfNotExists(verticesInNewMesh, vertex2 - normal2*4, newMesh);
            var intAInnerOldMesh = StaticFunctions.AppendIfNotExists(verticesInOldMesh, vertex1 - normal1 * 4, mesh);
            var intBInnerOldMesh = StaticFunctions.AppendIfNotExists(verticesInOldMesh, vertex2 - normal2 * 4, mesh);
            var intCInnerOldMesh = StaticFunctions.AppendIfNotExists(verticesInOldMesh, vertex3 - normal3 * 4, mesh);
            newMesh.SetVertexColor(intAInner, ColorManager.Instance.currentColor.toVector3f());
            newMesh.SetVertexColor(intBInner, ColorManager.Instance.currentColor.toVector3f());
            newMesh.SetVertexColor(intCInner, ColorManager.Instance.currentColor.toVector3f());
            mesh.SetVertexColor(intAInnerOldMesh, ColorManager.Instance.currentColor.toVector3f());
            mesh.SetVertexColor(intBInnerOldMesh, ColorManager.Instance.currentColor.toVector3f());
            mesh.SetVertexColor(intCInnerOldMesh, ColorManager.Instance.currentColor.toVector3f());
            var newTriInner = newMesh.AppendTriangle(intAInner, intBInner, intCInner, currentGid);
            var newTriInnerOldMesh = mesh.AppendTriangle(intAInnerOldMesh, intBInnerOldMesh, intCInnerOldMesh, 0);
            InnerToOuter.AddIfNotExists(intAOuter, intAInner);
            InnerToOuter.AddIfNotExists(intBOuter, intCInner);
            InnerToOuter.AddIfNotExists(intCOuter, intBInner);
            InnerToOuterOldMesh.AddIfNotExists(triangle.a, intAInnerOldMesh);
            InnerToOuterOldMesh.AddIfNotExists(triangle.b, intBInnerOldMesh);
            InnerToOuterOldMesh.AddIfNotExists(triangle.c, intCInnerOldMesh);
        }
        painted.ForEach(index => mesh.RemoveTriangle(index));

        var openEdges = newMesh.BoundaryEdgeIndices();
        foreach (var openEdge in openEdges)
        {
            var edgeOriented = newMesh.GetOrientedBoundaryEdgeV(openEdge);
            int thirdPoint = Corresponding(InnerToOuter,edgeOriented.a);
            var newTriSide = newMesh.AppendTriangle(edgeOriented.b, edgeOriented.a, thirdPoint, currentGid);
        }
        var openEdgesOldMesh = mesh.BoundaryEdgeIndices();
        foreach (var openEdge in openEdgesOldMesh)
        {
            var edgeOriented = mesh.GetOrientedBoundaryEdgeV(openEdge);
            int thirdPoint = Corresponding(InnerToOuterOldMesh, edgeOriented.a);
            var newTriSide = mesh.AppendTriangle(edgeOriented.b, edgeOriented.a, thirdPoint, 0);
        }

        var newObj = StaticFunctions.SpawnNewObject(newMesh);
        return mesh;
    }

    private int Corresponding(Dictionary<int, int> InnerToOuter, int searchFor)
    {
        if (InnerToOuter.ContainsKey(searchFor)) return InnerToOuter[searchFor];
        if (InnerToOuter.ContainsValue(searchFor))
            return InnerToOuter.First(tuple => tuple.Value == searchFor).Key;
        else return Int32.MaxValue;

    }
}