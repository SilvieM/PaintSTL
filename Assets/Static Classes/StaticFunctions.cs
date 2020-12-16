using System.Collections.Generic;
using g3;
using UnityEngine;

namespace Assets.Static_Classes
{
    public class StaticFunctions : MonoBehaviour
    {
        public static GameObject SpawnNewObject(DMesh3 mesh, string name="CutObject", bool isImported = false)
        {
            var res = Resources.Load("STLMeshMaterial2") as Material;
            var obj = g3UnityUtils.g3UnityUtils.CreateMeshGO(name, mesh, null, res);
            obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            if(isImported){
                obj.AddComponent<OnMeshClick>(); //cutted objects should not be painted on
                obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }
            if (!isImported)
            {
                obj.transform.SetParent(GameObject.FindGameObjectWithTag("mainObject").transform);
                obj.transform.localScale = Vector3.one;
            }
            obj.transform.localPosition = Vector3.zero;
            obj.AddComponent<Generate>().MyInit(mesh, isImported);
            return obj;
        }

        public static int AppendIfNotExists(Dictionary<Vector3d, int> verticesInNewMesh, Vector3d position, DMesh3 newMesh)
        {
            if (!verticesInNewMesh.TryGetValue(position, out var intA))
            {
                intA = newMesh.AppendVertex(position);
                verticesInNewMesh.Add(position, intA);
            }
            return intA;
        }

        

        public static void ErrorMessage(string message)
        {
            GameObject.FindObjectOfType<InterfaceFunctions>().ErrorMessage(message);
        }
    }
}