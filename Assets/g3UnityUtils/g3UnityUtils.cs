using System;
using System.Collections.Generic;
using System.Linq;
using g3;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.g3UnityUtils
{

    public static class g3UnityUtils
    {


        public static GameObject CreateMeshGO(string name, DMesh3 mesh, List<Color> colors = null, Material setMaterial = null, bool bCollider = true)
        {
            var gameObj = new GameObject(name);
            gameObj.AddComponent<MeshFilter>();
            SetGOMesh(gameObj, mesh);
            if (bCollider)
            {
                gameObj.AddComponent(typeof(MeshCollider));
                gameObj.GetComponent<MeshCollider>().enabled = true;
            }
            if (setMaterial)
            {
                gameObj.AddComponent<MeshRenderer>().material = setMaterial;
            }
            else
            {
                gameObj.AddComponent<MeshRenderer>().material = StandardMaterial(Color.red);
            }
            return gameObj;

        }


        public static DMesh3 SetGOMesh(GameObject go, DMesh3 mesh, List<Color> colors = null)
        {
            DMesh3 useMesh = mesh;
            if (!mesh.IsCompact)
            {
                useMesh = new DMesh3(mesh, true);
            }


            MeshFilter filter = go.GetComponent<MeshFilter>();
            if (filter == null)
                throw new Exception("g3UnityUtil.SetGOMesh: go " + go.name + " has no MeshFilter");
            Mesh unityMesh = DMeshToUnityMesh(useMesh, colors);
            filter.sharedMesh = unityMesh;
            MeshCollider collider = go.GetComponent<MeshCollider>();
            if (collider != null) collider.sharedMesh = unityMesh;
            return useMesh;
        }




        /// <summary>
        /// Convert DMesh3 to unity Mesh
        /// </summary>
        public static Mesh DMeshToUnityMesh(DMesh3 m, List<Color> colors = null, bool bLimitTo64k = false)
        {
            if (bLimitTo64k && (m.MaxVertexID > 65535 || m.MaxTriangleID > 65535))
            {
                Debug.Log("g3UnityUtils.DMeshToUnityMesh: attempted to convert DMesh larger than 65535 verts/tris, not supported by Unity!");
                return null;
            }

            Mesh unityMesh = new Mesh()
            {
                indexFormat = IndexFormat.UInt32
            };
            var vertices = new List<Vector3>();
            var verticesAsVec3 = toVector3(m.VerticesBuffer);
            var triangles = new List<int>();
            foreach (var triangle in m.Triangles())
            {
                vertices.Add(verticesAsVec3[triangle.a]);
                vertices.Add(verticesAsVec3[triangle.b]);
                vertices.Add(verticesAsVec3[triangle.c]);
                var index = triangles.Count;
                triangles.Add(index);
                triangles.Add(index+1);
                triangles.Add(index+2);
            }

            unityMesh.vertices = vertices.ToArray();
            //if (m.HasVertexNormals)
            //    unityMesh.normals = (m.HasVertexNormals) ? toVector3Array(m.NormalsBuffer) : null;
            //if (m.HasVertexColors)
              //  unityMesh.colors = dvector_to_color(m.ColorsBuffer);
            if (m.HasVertexUVs)
                unityMesh.uv = toVector2Array(m.UVBuffer);
            //unityMesh.triangles = dvector_to_int(m.TrianglesBuffer);
            unityMesh.triangles = triangles.ToArray();
            if (colors != null)
            {
                if (colors.Count == unityMesh.vertexCount) unityMesh.colors = colors.ToArray();
                else if(colors.Count*3==unityMesh.vertexCount)
                {
                    var tripleColors = colors.SelectMany(color => new List<Color>() {color, color, color}).ToArray();
                    unityMesh.colors = tripleColors;
                }
                else
                {
                    Debug.Log($"Vertices: {unityMesh.vertices.Length} Colors: {colors.Count}");
                }
            }
            else unityMesh.colors = Enumerable.Repeat(Color.white, unityMesh.vertexCount).ToArray();
            //if (m.HasVertexNormals == false)
                unityMesh.RecalculateNormals(); //TODO

            return unityMesh;
        }


        ///// <summary>
        ///// Convert unity Mesh to a g3.DMesh3. Ignores UV's.
        ///// </summary>
        //public static DMesh3 UnityMeshToDMesh(Mesh mesh)
        //{
        //    Vector3[] mesh_vertices = mesh.vertices;
        //    Vector3f[] dmesh_vertices = new Vector3f[mesh_vertices.Length];
        //    for (int i = 0; i < mesh.vertexCount; ++i)
        //        dmesh_vertices[i] = mesh_vertices[i];

        //    Vector3[] mesh_normals = mesh.normals;
        //    if (mesh_normals != null)
        //    {
        //        Vector3f[] dmesh_normals = new Vector3f[mesh_vertices.Length];
        //        for (int i = 0; i < mesh.vertexCount; ++i)
        //            dmesh_normals[i] = mesh_normals[i];

        //        return DMesh3Builder.Build(dmesh_vertices, mesh.triangles, dmesh_normals);

        //    }
        //    else
        //    {
        //        return DMesh3Builder.Build<Vector3f, int, Vector3f>(dmesh_vertices, mesh.triangles, null, null);
        //    }
        //}



        public static Material StandardMaterial(Color color)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            return mat;
        }


        public static Material SafeLoadMaterial(string sPath)
        {
            Material mat = null;
            try
            {
                Material loaded = Resources.Load<Material>(sPath);
                mat = new Material(loaded);
            }
            catch (Exception e)
            {
                Debug.Log("g3UnityUtil.SafeLoadMaterial: exception: " + e.Message);
                mat = new Material(Shader.Find("Standard"));
                mat.color = Color.red;
            }
            return mat;
        }






        // per-type conversion functions
        public static Vector3[] toVector3(this DVector<double> vec)
        {
            int nLen = vec.Length / 3;
            Vector3[] result = new Vector3[nLen];
            for (int i = 0; i < nLen; ++i)
            {
                result[i].x = (float)vec[3 * i];
                result[i].y = (float)vec[3 * i + 1];
                result[i].z = (float)vec[3 * i + 2];
            }
            return result;
        }

        public static DVector<double> to_dVector(this Vector3 vec)
        {
            return new DVector<double>(new double[]{vec.x, vec.y, vec.z});
        }

        public static Vector3d toVector3d(this Vector3 vec)
        {
            return new Vector3d(new double[] { vec.x, vec.y, vec.z });
        }
        public static Vector3 toVector3(this Vector3d vec)
        {
            return new Vector3((float)vec.x, (float)vec.y, (float)vec.z);
        }

        public static Vector3? toOneVector3(this DVector<double> vec)
        {
            if (vec.Length != 3) return null;
            Vector3 result = new Vector3();
            result.x = (float)vec[0];
            result.y = (float)vec[1];
            result.z = (float)vec[2];
            
            return result;
        }
        public static Vector3[] toVector3Array(this DVector<float> vec)
        {
            int nLen = vec.Length / 3;
            Vector3[] result = new Vector3[nLen];
            for (int i = 0; i < nLen; ++i)
            {
                result[i].x = vec[3 * i];
                result[i].y = vec[3 * i + 1];
                result[i].z = vec[3 * i + 2];
            }
            return result;
        }
        public static Vector2[] toVector2Array(this DVector<float> vec)
        {
            int nLen = vec.Length / 2;
            Vector2[] result = new Vector2[nLen];
            for (int i = 0; i < nLen; ++i)
            {
                result[i].x = vec[2 * i];
                result[i].y = vec[2 * i + 1];
            }
            return result;
        }
        public static Color[] dvector_to_color(this DVector<float> vec)
        {
            int nLen = vec.Length / 3;
            Color[] result = new Color[nLen];
            for (int i = 0; i < nLen; ++i)
            {
                result[i].r = vec[3 * i];
                result[i].g = vec[3 * i + 1];
                result[i].b = vec[3 * i + 2];
            }
            return result;
        }
        public static int[] dvector_to_int(this DVector<int> vec)
        {
            // todo this could be faster because we can directly copy chunks...
            int nLen = vec.Length;
            int[] result = new int[nLen];
            for (int i = 0; i < nLen; ++i)
                result[i] = vec[i];
            return result;
        }


    }
}