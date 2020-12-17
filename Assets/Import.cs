using System.Collections;
using System.IO;
using Assets;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;
using gs;
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

        //if(readMesh.CheckValidity()) StaticFunctions.ErrorMessage("Imported Model has errors");
        var filename = Path.GetFileNameWithoutExtension(path);
        readMesh.EnableTriangleGroups();
        readMesh.EnableVertexColors(new Vector3f(1, 1, 1));
        readMesh.EnableVertexNormals(Vector3f.One);

        foreach (var vertexIndex in readMesh.VertexIndices())
        {
            readMesh.SetVertexNormal(vertexIndex, readMesh.CalcVertexNormal(vertexIndex).toVector3f());
        }
        
        var obj = StaticFunctions.SpawnNewObject(readMesh, filename, true);
        var center = obj.GetComponent<Generate>().centerInWorldCoords;
        obj.transform.position = -center; //position object such that center is zero.
        obj.tag = "mainObject";
        var viewDistance = obj.transform.TransformPoint(readMesh.GetBounds().Extents.toVector3()) * 2;
        Camera.main.GetComponent<OrbitingCam>().SetTarget(obj, viewDistance);
        
        DebugGizmos.DrawBoundingBox(readMesh.GetBounds(), obj.transform);
        //Camera.main.GetComponent<SimpleCameraController>().m_TargetCameraState.SetFromTransform(Camera.main.transform);
        yield return null;
    }
}
