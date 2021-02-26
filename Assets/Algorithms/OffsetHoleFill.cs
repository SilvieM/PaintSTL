using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;

namespace Assets.Algorithms
{
    public class OffsetHoleFill : Algorithm
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
                BuildStructure(info, newMesh, info.data.ColorNum);
                BuildStructure(info, info.mesh, info.data.mainColorId);

                var newObj = StaticFunctions.SpawnNewObject(newMesh);
            }

            return info.mesh;
        }

        private static void BuildStructure(CuttingInfo info, DMesh3 mesh, int color)
        {
            var loops = new MeshBoundaryLoops(mesh, true);
            var meshEditor = new MeshEditor(mesh);
            foreach (var meshBoundaryLoop in loops)
            {
                var offsettedVertices = new List<int>();
                foreach (var vertex in meshBoundaryLoop.Vertices)
                {
                    var normal = mesh.GetVertexNormal(vertex);
                    var vertextPosition = mesh.GetVertex(vertex);
                    var newVertex = mesh.AppendVertex(vertextPosition - normal.toVector3d() * info.data.depth); //depth here
                    offsettedVertices.Add(newVertex);
                }

                meshEditor.StitchLoop(meshBoundaryLoop.Vertices, offsettedVertices.ToArray(), color);

                var offsettedLoop = EdgeLoop.FromVertices(mesh, offsettedVertices);
                var holeFiller = new SimpleHoleFiller(mesh, offsettedLoop);
                var valid = holeFiller.Validate();
                if (valid == ValidationStatus.Ok)
                {
                    var res = holeFiller.Fill(color);
                }
            }
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
