using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Classes;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;

namespace Assets.Algorithms
{
    class HoleFillAlgorithm : Algorithm
    {
        public override DMesh3 Cut(CuttingInfo info)
        {
            var painted = FindPaintedTriangles(info.mesh, info.data.ColorNum);
            if (painted.Count <= 0) return info.mesh;

            var components = new MeshConnectedComponents(info.mesh);
            components.FilterF = i => info.mesh.GetTriangleGroup(i) == info.data.ColorNum;
            components.FindConnectedT();
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
                    }
                }

                var loopsOldMesh = new MeshBoundaryLoops(info.mesh, true);
                foreach (var meshBoundaryLoop in loopsOldMesh)
                {
                    var holeFiller = new SimpleHoleFiller(info.mesh, meshBoundaryLoop);
                    var valid = holeFiller.Validate();
                    if (valid == ValidationStatus.Ok)
                    {
                        var res = holeFiller.Fill(0);
                    }
                }








                var newObj = StaticFunctions.SpawnNewObject(newMesh);
            }



            return info.mesh;
        }


        private void AddTriangle(DMesh3 currentMesh, int openEdge, int centerPoint, int currentGid)
        {
            var edge = currentMesh.GetOrientedBoundaryEdgeV(openEdge);
            currentMesh.AppendTriangle(edge.b, edge.a, centerPoint, currentGid);
        }
    }
}
