using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Assets;
using Assets.Algorithms;
using Assets.Classes;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;
using UnityEngine;

public class BacksideAlgorithm : Algorithm
{

    public class PeprStatusVert
    {
        /// <summary>
        /// The outer vert in old mesh that might be deleted
        /// </summary>
        public int idOldMeshOuter;
        /// <summary>
        /// the outer vert in new mesh (direct copy of deleted one)
        /// </summary>
        public int? idNewMeshOuter;
        /// <summary>
        /// the newly generated (offsetted) vert in old mesh
        /// </summary>
        public int? idOldMeshInner;
        /// <summary>
        /// the inner (cut-side) vert in new mesh
        /// </summary>
        public int? idNewMeshInner;
    }

    public override DMesh3 Cut(CuttingInfo info)
    {
        var painted = FindPaintedTriangles(info.mesh, info.data.ColorNum);
        if (painted.Count <= 0) return info.mesh;
        var newMesh = new DMesh3();
        newMesh.EnableTriangleGroups();
        newMesh.EnableVertexColors(new Vector3f(1, 1, 1));

        var stati = new Dictionary<int,PeprStatusVert>();

        foreach (var triIndex in painted)
        {
            var triangle = info.mesh.GetTriangle(triIndex);
            var vertex1 = info.mesh.GetVertex(triangle.a);
            var vertex2 = info.mesh.GetVertex(triangle.b);
            var vertex3 = info.mesh.GetVertex(triangle.c);
            stati.AppendIfNotExists(triangle.a);
            stati.AppendIfNotExists(triangle.b);
            stati.AppendIfNotExists(triangle.c);

            if(stati[triangle.a].idNewMeshOuter==null) stati[triangle.a].idNewMeshOuter = newMesh.AppendVertex(vertex1);
            if (stati[triangle.b].idNewMeshOuter == null) stati[triangle.b].idNewMeshOuter = newMesh.AppendVertex(vertex2);
            if (stati[triangle.c].idNewMeshOuter == null) stati[triangle.c].idNewMeshOuter = newMesh.AppendVertex(vertex3);

            var newTriOuter = newMesh.AppendTriangle(stati[triangle.a].idNewMeshOuter.Value, stati[triangle.b].idNewMeshOuter.Value, stati[triangle.c].idNewMeshOuter.Value, info.data.ColorNum);
            

            var normal1 = info.mesh.GetVertexNormal(triangle.a).toVector3d();
            var normal2 = info.mesh.GetVertexNormal(triangle.b).toVector3d();
            var normal3 = info.mesh.GetVertexNormal(triangle.c).toVector3d();

            
            var pos1 = vertex1 - normal1 * info.data.depth;
            var pos2 = vertex2 - normal2 * info.data.depth;
            var pos3 = vertex3 - normal3 * info.data.depth;

            if (stati[triangle.a].idNewMeshInner == null) stati[triangle.a].idNewMeshInner = newMesh.AppendVertex(pos1); 
            if (stati[triangle.b].idNewMeshInner == null) stati[triangle.b].idNewMeshInner = newMesh.AppendVertex(pos2);
            if (stati[triangle.c].idNewMeshInner == null) stati[triangle.c].idNewMeshInner = newMesh.AppendVertex(pos3); //TODO it was flipped from here before, but we have to flip when making the triangles
            if (stati[triangle.a].idOldMeshInner == null) stati[triangle.a].idOldMeshInner = info.mesh.AppendVertex(pos1);
            if (stati[triangle.b].idOldMeshInner == null) stati[triangle.b].idOldMeshInner = info.mesh.AppendVertex(pos2);
            if (stati[triangle.c].idOldMeshInner == null) stati[triangle.c].idOldMeshInner = info.mesh.AppendVertex(pos3);


            var color = ColorManager.Instance.GetColorForId(info.data.ColorNum).toVector3f();
            newMesh.SetVertexColor(stati[triangle.a].idNewMeshInner.Value, color);
            newMesh.SetVertexColor(stati[triangle.b].idNewMeshInner.Value, color);
            newMesh.SetVertexColor(stati[triangle.c].idNewMeshInner.Value, color);
            info.mesh.SetVertexColor(stati[triangle.a].idOldMeshInner.Value, color);
            info.mesh.SetVertexColor(stati[triangle.b].idOldMeshInner.Value, color);
            info.mesh.SetVertexColor(stati[triangle.c].idOldMeshInner.Value, color);
            var newTriInner = newMesh.AppendTriangle(stati[triangle.a].idNewMeshInner.Value, stati[triangle.c].idNewMeshInner.Value, stati[triangle.b].idNewMeshInner.Value, info.data.ColorNum);
            var newTriInnerOldMesh = info.mesh.AppendTriangle(stati[triangle.a].idOldMeshInner.Value, stati[triangle.b].idOldMeshInner.Value, stati[triangle.c].idOldMeshInner.Value, 0);
        }
        if (info.data.modifier == CutSettingData.Modifier.DepthDependant) MoveAllPointsDepthDependant(info, newMesh, stati);
        if (info.data.modifier == CutSettingData.Modifier.Compute) MoveVerticesToValidPositions(info, newMesh, stati); //TODO
        painted.ForEach(index => info.mesh.RemoveTriangle(index));

        var openEdges = newMesh.BoundaryEdgeIndices();
        foreach (var openEdge in openEdges)
        {
            var edgeOriented = newMesh.GetOrientedBoundaryEdgeV(openEdge);
            int thirdPoint = Corresponding(stati,edgeOriented.a, true);
            var newTriSide = newMesh.AppendTriangle(edgeOriented.b, edgeOriented.a, thirdPoint, info.data.ColorNum);
        }
        var openEdgesOldMesh = info.mesh.BoundaryEdgeIndices();
        foreach (var openEdge in openEdgesOldMesh)
        {
            var edgeOriented = info.mesh.GetOrientedBoundaryEdgeV(openEdge);
            int thirdPoint = Corresponding(stati, edgeOriented.a, false);
            var newTriSide = info.mesh.AppendTriangle(edgeOriented.b, edgeOriented.a, thirdPoint, 0);
        }
        
        
        var newObj = StaticFunctions.SpawnNewObject(newMesh);
        return info.mesh;
    }

    private void MoveVerticesToValidPositions(CuttingInfo info, DMesh3 newMesh, Dictionary<int, PeprStatusVert> stati)
    {
        foreach (var status in stati)
        {
            var point = info.mesh.GetVertex(status.Value.idOldMeshInner.Value);
            var newPoint = MovePointInsideAndAwayFromShell(info, point);
            info.mesh.SetVertex(status.Value.idOldMeshInner.Value, newPoint);
            newMesh.SetVertex(status.Value.idNewMeshInner.Value, newPoint);

        }
    }

    private int Corresponding(Dictionary<int, PeprStatusVert> stati, int searchFor, bool inNewMesh)
    {
        if (inNewMesh)
        {
            var found = stati.FirstOrDefault(status => status.Value.idNewMeshInner == searchFor);
            if (found.Key != 0) return found.Value.idNewMeshOuter.Value;
            var found2 = stati.FirstOrDefault(status => status.Value.idNewMeshOuter == searchFor);
            if (found2.Key !=0) return found2.Value.idNewMeshInner.Value;
        }
        else
        {
            if (stati.ContainsKey(searchFor)) return stati[searchFor].idOldMeshInner.Value;
            var found = stati.FirstOrDefault(status => status.Value.idOldMeshInner == searchFor);
            if (found.Key != 0) return found.Value.idOldMeshOuter;
        }
        Debug.Log("Corresponding failed");
        return Int32.MaxValue;
    }
}