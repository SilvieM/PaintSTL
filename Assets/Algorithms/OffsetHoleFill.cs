using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;
using UnityEngine;

namespace Assets.Algorithms
{
    public class OffsetHoleFill : Algorithm
    {
        public override DMesh3 Cut(CuttingInfo info)
        {
            var painted = FindPaintedTriangles(info.mesh, info.data.ColorNum);
            if (painted.Count <= 0) return info.mesh;

                DSubmesh3 subMesh = new DSubmesh3(info.mesh, painted);
                var newMesh = subMesh.SubMesh;
                newMesh.EnableTriangleGroups();
                newMesh.EnableVertexColors(ColorManager.Instance.GetColorForId(info.data.ColorNum).toVector3f());
                foreach (var componentIndex in painted)
                {
                    info.mesh.RemoveTriangle(componentIndex);
                }
                var pointToPoint = new Dictionary<int, int>();

                var loops = new MeshBoundaryLoops(newMesh, true);
                var meshEditorNewMesh = new MeshEditor(newMesh);
                var meshEditorOldMesh = new MeshEditor(info.mesh);
                foreach (var meshBoundaryLoop in loops)
                {
                    var offsettedVerticesNewMesh = new List<int>();
                    var offsettedVerticesOldMesh = new List<int>();
                    foreach (var vertex in meshBoundaryLoop.Vertices)
                    {
                        var normal = newMesh.GetVertexNormal(vertex);
                        var vertextPosition = newMesh.GetVertex(vertex);
                        var newVertex = newMesh.AppendVertex(vertextPosition - normal.toVector3d() * info.data.depth);
                        offsettedVerticesNewMesh.Add(newVertex);
                        var newVertexOldMesh = info.mesh.AppendVertex(vertextPosition - normal.toVector3d() * info.data.depth);
                        offsettedVerticesOldMesh.Add(newVertexOldMesh);
                    }

                    var boundaryLoopOfOldMesh = meshBoundaryLoop.Vertices
                        .Select(subv => subMesh.MapVertexToBaseMesh(subv)).ToArray();
                    var loopNewMesh = meshEditorNewMesh.StitchLoop(meshBoundaryLoop.Vertices, offsettedVerticesNewMesh.ToArray(), info.data.ColorNum);
                    var loopOldMesh = meshEditorOldMesh.StitchLoop(boundaryLoopOfOldMesh, offsettedVerticesOldMesh.ToArray(), info.data.mainColorId);

                    for (var index = 0; index < offsettedVerticesNewMesh.Count; index++)
                    {
                        info.PointToPoint.Add(offsettedVerticesNewMesh[index], offsettedVerticesOldMesh[index]);
                    }

                    var offsettedLoop = EdgeLoop.FromVertices(newMesh, offsettedVerticesNewMesh);
                    var holeFiller = new SimpleHoleFiller(newMesh, offsettedLoop);
                    var valid = holeFiller.Validate();
                    if (valid == ValidationStatus.Ok)
                    {
                        var res = holeFiller.Fill(info.data.ColorNum);
                        if (res)
                        {
                            var newVertex = holeFiller.NewVertex;
                            var newTriangles = holeFiller.NewTriangles;

                            //Add the same triangles to old mesh.
                            if (newVertex == -1) //case where it added only one tri
                            {
                                var vertices = newMesh.GetTriangle(newTriangles.First());
                                var edgeAOldMesh = info.PointToPoint[vertices.a];
                                var edgeBOldMesh = info.PointToPoint[vertices.b];
                                var edgeCOldMesh = info.PointToPoint[vertices.c];
                                info.mesh.AppendTriangle(edgeAOldMesh, edgeCOldMesh, edgeBOldMesh);
                            }
                            else //case where multiple tris and a middle vertex were added
                            {
                                var newVertexOldMesh = info.mesh.AppendVertex(newMesh.GetVertex(newVertex));
                                foreach (var newTriangle in newTriangles)
                                {
                                    //the center is always the first vertex in newTriangle
                                    var edgeVertices = newMesh.GetTriangle(newTriangle);
                                    var edgeBOldMesh = info.PointToPoint[edgeVertices.b];
                                    var edgeCOldMesh = info.PointToPoint[edgeVertices.c];
                                    info.mesh.AppendTriangle(newVertexOldMesh, edgeCOldMesh, edgeBOldMesh,
                                        info.data.mainColorId);
                                }

                                if (info.PointToPoint.ContainsKey(newVertex))
                                    Debug.Log($"Double insertion from HF: {newVertex}, {newVertexOldMesh}");
                                else info.PointToPoint.Add(newVertex, newVertexOldMesh);
                            }
                        }
                    }
                }

                var newObj = StaticFunctions.SpawnNewObject(newMesh);
                newObj.GetComponent<Generate>().cuttingInfo = info;
            

            return info.mesh;
        }

        

        private bool IsInRange(DMesh3 mesh, int triIndexOriginal, int triIndex, double rangeSquared)
        {
            var triOriginal = mesh.GetTriCentroid(triIndexOriginal);
            var tri = mesh.GetTriangle(triIndex);
            var v1 = mesh.GetVertex(tri.a);
            var v2 = mesh.GetVertex(tri.b);
            var v3 = mesh.GetVertex(tri.c);
            if (v1.DistanceSquared(triOriginal) < rangeSquared) return true;
            if (v2.DistanceSquared(triOriginal) < rangeSquared) return true;
            if (v3.DistanceSquared(triOriginal) < rangeSquared) return true;
            return false;
        }
    }
}
