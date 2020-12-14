using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Assets.Algorithms;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;
using UnityEngine;

public class PeprAlgorithm : Algorithm
{
    public override DMesh3 Cut(CuttingInfo info)
    {
        var painted = FindPaintedTriangles(info.mesh, info.colorId);
        if (painted.Count <= 0) return info.mesh;
        var newMesh = new DMesh3();
        newMesh.EnableTriangleGroups();
        newMesh.EnableVertexColors(new Vector3f(1, 1, 1));

        var verticesInNewMesh = new Dictionary<Vector3d, int>();
        var verticesInOldMesh = new Dictionary<Vector3d, int>();
        var InnerToOuter = new Dictionary<int, int>();
        var InnerToOuterOldMesh = new Dictionary<int, int>();
        foreach (var triIndex in painted)
        {
            var triangle = info.mesh.GetTriangle(triIndex);
            var vertex1 = info.mesh.GetVertex(triangle.a);
            var vertex2 = info.mesh.GetVertex(triangle.b);
            var vertex3 = info.mesh.GetVertex(triangle.c);

            var intAOuter = StaticFunctions.AppendIfNotExists(verticesInNewMesh, vertex1, newMesh);
            var intBOuter = StaticFunctions.AppendIfNotExists(verticesInNewMesh, vertex2, newMesh); 
            var intCOuter = StaticFunctions.AppendIfNotExists(verticesInNewMesh, vertex3, newMesh);

            var newTriOuter = newMesh.AppendTriangle(intAOuter, intBOuter, intCOuter, info.colorId);

            var normal1 = info.mesh.CalcVertexNormal(triangle.a);
            var normal2 = info.mesh.CalcVertexNormal(triangle.b);
            var normal3 = info.mesh.CalcVertexNormal(triangle.c);

            
            var pos1 = vertex1 - normal1 * info.depth;
            var pos2 = vertex2 - normal2 * info.depth;
            var pos3 = vertex3 - normal3 * info.depth;
            //if (info.modelDepthDependantDepth)
            //{
            //    pos1 = MovePointDepthDependant(info, vertex1, normal1);
            //    pos2 = MovePointDepthDependant(info, vertex2, normal2);
            //    pos3 = MovePointDepthDependant(info, vertex3, normal3);
            //}

            var intAInner = StaticFunctions.AppendIfNotExists(verticesInNewMesh, pos1, newMesh);
            var intBInner = StaticFunctions.AppendIfNotExists(verticesInNewMesh, pos3, newMesh); //swapping to mirror
            var intCInner = StaticFunctions.AppendIfNotExists(verticesInNewMesh, pos2, newMesh);
            var intAInnerOldMesh = StaticFunctions.AppendIfNotExists(verticesInOldMesh, pos1, info.mesh);
            var intBInnerOldMesh = StaticFunctions.AppendIfNotExists(verticesInOldMesh, pos2, info.mesh);
            var intCInnerOldMesh = StaticFunctions.AppendIfNotExists(verticesInOldMesh, pos3, info.mesh);
            var color = ColorManager.Instance.GetColorForId(info.colorId).toVector3f();
            newMesh.SetVertexColor(intAInner, color);
            newMesh.SetVertexColor(intBInner, color);
            newMesh.SetVertexColor(intCInner, color);
            info.mesh.SetVertexColor(intAInnerOldMesh, color);
            info.mesh.SetVertexColor(intBInnerOldMesh, color);
            info.mesh.SetVertexColor(intCInnerOldMesh, color);
            var newTriInner = newMesh.AppendTriangle(intAInner, intBInner, intCInner, info.colorId);
            var newTriInnerOldMesh = info.mesh.AppendTriangle(intAInnerOldMesh, intBInnerOldMesh, intCInnerOldMesh, 0);
            InnerToOuter.AddIfNotExists(intAOuter, intAInner);
            InnerToOuter.AddIfNotExists(intBOuter, intCInner);
            InnerToOuter.AddIfNotExists(intCOuter, intBInner);
            InnerToOuterOldMesh.AddIfNotExists(triangle.a, intAInnerOldMesh);
            InnerToOuterOldMesh.AddIfNotExists(triangle.b, intBInnerOldMesh);
            InnerToOuterOldMesh.AddIfNotExists(triangle.c, intCInnerOldMesh);
        }
        painted.ForEach(index => info.mesh.RemoveTriangle(index));

        var openEdges = newMesh.BoundaryEdgeIndices();
        foreach (var openEdge in openEdges)
        {
            var edgeOriented = newMesh.GetOrientedBoundaryEdgeV(openEdge);
            int thirdPoint = Corresponding(InnerToOuter,edgeOriented.a);
            var newTriSide = newMesh.AppendTriangle(edgeOriented.b, edgeOriented.a, thirdPoint, info.colorId);
        }
        var openEdgesOldMesh = info.mesh.BoundaryEdgeIndices();
        foreach (var openEdge in openEdgesOldMesh)
        {
            var edgeOriented = info.mesh.GetOrientedBoundaryEdgeV(openEdge);
            int thirdPoint = Corresponding(InnerToOuterOldMesh, edgeOriented.a);
            var newTriSide = info.mesh.AppendTriangle(edgeOriented.b, edgeOriented.a, thirdPoint, 0);
        }
        if (info.computeCorrectPosition) MoveVerticesToValidPositions(info, newMesh, verticesInNewMesh, verticesInOldMesh);
        var newObj = StaticFunctions.SpawnNewObject(newMesh);
        return info.mesh;
    }

    private void MoveVerticesToValidPositions(CuttingInfo info, DMesh3 newMesh, Dictionary<Vector3d, int> verticesInNewMesh, Dictionary<Vector3d, int> verticesInOldMesh)
    {
        foreach (var keyValuePair in verticesInOldMesh)
        {
            var point = keyValuePair.Key;
            var newPoint = MovePointInsideAndAwayFromShell(info, point);
            info.mesh.SetVertex(keyValuePair.Value, newPoint);
            var vidInNewMesh =verticesInNewMesh[point];
            newMesh.SetVertex(vidInNewMesh, newPoint);

        }
    }

    private int Corresponding(Dictionary<int, int> InnerToOuter, int searchFor)
    {
        if (InnerToOuter.ContainsKey(searchFor)) return InnerToOuter[searchFor];
        if (InnerToOuter.ContainsValue(searchFor))
            return InnerToOuter.First(tuple => tuple.Value == searchFor).Key;
        else return Int32.MaxValue;

    }
}