using g3;
using UnityEngine;

namespace Assets.Static_Classes
{
    public class StaticFunctions : MonoBehaviour
    {
        public static GameObject SpawnNewObject(DMesh3 mesh, DMesh3 originalMesh = null)
        {
            var res = Resources.Load("STLMeshMaterial2") as Material;
            var obj = g3UnityUtils.g3UnityUtils.CreateMeshGO("ImportedObject", mesh, null, res);
            obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            obj.AddComponent<OnMeshClick>();
            obj.AddComponent<Generate>().MyInit(mesh, originalMesh);
            return obj;
        }
    }
}