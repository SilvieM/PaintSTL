using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.Classes;
using g3;

namespace Assets
{
    public static class MyExtensions
    {
        public static IEnumerable<(T item, int index)> LoopIndex<T>(this IEnumerable<T> self) =>
            self.Select((item, index) => (item, index));


        public static void AddIfNotExists(this Dictionary<int, int> dictionary, int key, int value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary[key] = value;
        }

        public static Vector3 Average(this IEnumerable<Vector3> source)
        {
            var x = source.Average(vec => vec.x);
            var y = source.Average(vec => vec.y);
            var z = source.Average(vec => vec.z);
            return new Vector3(x,y,z);
        }
        public static Vector3d AverageVec3d(this IEnumerable<Vector3d> source)
        {
            var x = source.Average(vec => vec.x);
            var y = source.Average(vec => vec.y);
            var z = source.Average(vec => vec.z);
            return new Vector3d(x, y, z);
        }

        public static Vector3d Average(this IEnumerable<Vector3d> source)
        {
            var x = source.Average(vec => vec.x);
            var y = source.Average(vec => vec.y);
            var z = source.Average(vec => vec.z);
            return new Vector3d(x, y, z);
        }

        public static Vector3d CalcVertexNormal(this DMesh3 mesh, int vid)
        {
            var tris = mesh.VtxTrianglesItr(vid);
            var normals = tris.Select(mesh.GetTriNormal);
            var avg = normals.ToList().Average();
            return avg;
        }
        public static PeprAlgorithm.PeprStatusVert AppendIfNotExists(this Dictionary<int, PeprAlgorithm.PeprStatusVert> stati, int index)
        {
            if (!stati.TryGetValue(index, out var statusVert))
            {
                statusVert = new PeprAlgorithm.PeprStatusVert() { idOldMeshOuter = index };
                stati.Add(index, statusVert);
            }
            return statusVert;
        }
    }
}