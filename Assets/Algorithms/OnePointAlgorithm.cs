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

        var components = FindConnectedComponents(info, painted);
        var subMeshes = new List<DMesh3>();
        foreach (var component in components)
        {
            DSubmesh3 subMesh = new DSubmesh3(info.mesh, component.Indices);
            var newMesh = subMesh.SubMesh;
            newMesh.EnableTriangleGroups();
            //newMesh.EnableVertexColors(new Vector3f(1, 1, 1));
            var normals = new List<Vector3d>();
            var vertices = new List<Vector3d>();
            var verticesInNewMesh = new Dictionary<Vector3d, int>();
            foreach (var componentTriIndex in component.Indices)
            {
                var tri = info.mesh.GetTriangle(componentTriIndex);
                var normal = info.mesh.GetTriNormal(componentTriIndex);
                normals.Add(normal);
                var orgA = info.mesh.GetVertex(tri.a);
                vertices.Add(orgA);
                var orgB = info.mesh.GetVertex(tri.b);
                vertices.Add(orgB);
                var orgC = info.mesh.GetVertex(tri.c);
                vertices.Add(orgC);
                //var intA = StaticFunctions.AppendIfNotExists(verticesInNewMesh, orgA, newMesh);
                //var intB = StaticFunctions.AppendIfNotExists(verticesInNewMesh, orgB, newMesh);
                //var intC = StaticFunctions.AppendIfNotExists(verticesInNewMesh, orgC, newMesh);

                //newMesh.AppendTriangle(intA, intB, intC, info.data.ColorNum);
            }

            var avgNormal = normals.Average();
            var avgVertices = vertices.Average();
            var newPoint = avgVertices - avgNormal * info.data.depth;

            if (info.data.modifier == CutSettingData.Modifier.Compute) newPoint = MovePointInsideAndAwayFromShell(info, newPoint);
            if (info.data.modifier == CutSettingData.Modifier.DepthDependant) newPoint = MovePointDepthDependant(info, avgVertices, avgNormal);
            component.Indices.ToList().ForEach(index => info.mesh.RemoveTriangle(index));


            var newPointId = newMesh.AppendVertex(newPoint);
            var newPointIdInOldMesh = info.mesh.AppendVertex(newPoint);
            info.PointToPoint.Add(newPointId, newPointIdInOldMesh);

            //var eids = info.mesh.BoundaryEdgeIndices().ToList();
            //foreach (var openEdge in eids)
            //{
            //    AddTriangle(info.mesh, openEdge, newPointIdInOldMesh, ColorManager.Instance.MainColorId);
            //}
            
            var eidsNewMesh = newMesh.BoundaryEdgeIndices().ToList();
            foreach (var openEdge in eidsNewMesh)
            {
                var edge = newMesh.GetOrientedBoundaryEdgeV(openEdge);
                newMesh.AppendTriangle(edge.b, edge.a, newPointId, info.data.ColorNum);
                info.mesh.AppendTriangle(subMesh.MapVertexToBaseMesh(edge.a), subMesh.MapVertexToBaseMesh(edge.b),
                    newPointIdInOldMesh, ColorManager.Instance.MainColorId);
            }

            //info.mesh.SetVertexColor(newPointIdInOldMesh, ColorManager.Instance.GetColorForId(info.data.ColorNum).toVector3f());

            //newMesh.SetVertexColor(newPointId, ColorManager.Instance.GetColorForId(info.data.ColorNum).toVector3f());
            subMeshes.Add(newMesh);
        }

        InstantiateNewObjects(info, subMeshes);
        

        return info.mesh;
    }


    private void AddTriangle(DMesh3 currentMesh, int openEdge, int centerPoint, int currentGid)
    {
        var edge = currentMesh.GetOrientedBoundaryEdgeV(openEdge);
        currentMesh.AppendTriangle(edge.b, edge.a, centerPoint, currentGid);
    }
}