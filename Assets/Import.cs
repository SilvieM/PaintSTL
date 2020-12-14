using System.Collections;
using Assets;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;
using UnityEditor;
using UnityEngine;
using UnityTemplateProjects;

public class Import : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ImportSTL()
    {
        string path = EditorUtility.OpenFilePanel("Import STL File", "", "stl");
        GameObject created = null;
        if (path.Length == 0)
        {
            return;
        }
        var coroutine = ReadMesh(path);
        StartCoroutine(coroutine);

    }

    private IEnumerator ReadMesh(string path)
    {
        DMesh3 readMesh = StandardMeshReader.ReadMesh(path);
        readMesh.EnableTriangleGroups();
        readMesh.EnableVertexColors(new Vector3f(1, 1, 1));
        StaticFunctions.SpawnNewObject(readMesh, true);
        var bounds = readMesh.GetBounds();
        Camera.main.transform.LookAt(bounds.Center.toVector3());

        Camera.main.GetComponent<SimpleCameraController>().m_TargetCameraState.SetFromTransform(Camera.main.transform);
        yield return null;
    }
}
