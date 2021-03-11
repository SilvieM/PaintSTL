using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Classes;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;
using UnityEngine;

namespace Assets.Algorithms
{
    class HoleFillAlgorithm : Algorithm
    {
        public override DMesh3 Cut(CuttingInfo info)
        {
            var painted = FindPaintedTriangles(info.mesh, info.data.ColorNum);
            if (painted.Count <= 0) return info.mesh;

            var components = FindConnectedComponents(info, painted);
            var subMeshes = new List<DMesh3>();
            foreach (var component in components.Components)
            {
                DSubmesh3 subMesh = new DSubmesh3(info.mesh, component.Indices);
                var newMesh = subMesh.SubMesh;
                newMesh.EnableTriangleGroups();
                newMesh.EnableVertexColors(ColorManager.Instance.GetColorForId(info.data.ColorNum).toVector3f());
                foreach (var componentIndex in component.Indices)
                {
                    info.mesh.RemoveTriangle(componentIndex);
                }


                var loops = new MeshBoundaryLoops(newMesh, true);
                foreach (var meshBoundaryLoop in loops)
                {
                    var holeFiller = new SimpleHoleFiller(newMesh, meshBoundaryLoop);
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
                                var edgeAOldMesh = subMesh.MapVertexToBaseMesh(vertices.a);
                                var edgeBOldMesh = subMesh.MapVertexToBaseMesh(vertices.b);
                                var edgeCOldMesh = subMesh.MapVertexToBaseMesh(vertices.c);
                                info.mesh.AppendTriangle(edgeAOldMesh, edgeCOldMesh, edgeBOldMesh);
                            }
                            else //case where multiple tris and a middle vertex were added
                            {
                                var newVertexOldMesh = info.mesh.AppendVertex(newMesh.GetVertex(newVertex));
                                foreach (var newTriangle in newTriangles)
                                {
                                    //the center is always the first vertex in newTriangle
                                    var edgeVertices = newMesh.GetTriangle(newTriangle);
                                    var edgeBOldMesh = subMesh.MapVertexToBaseMesh(edgeVertices.b);
                                    var edgeCOldMesh = subMesh.MapVertexToBaseMesh(edgeVertices.c);
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
                subMeshes.Add(newMesh);
            }

            InstantiateNewObjects(info, subMeshes);

            return info.mesh;
        }
    }
}
