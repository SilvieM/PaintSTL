using Assets.Static_Classes;
using g3;
using UnityEditor;
using UnityEngine;

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
        DMesh3 readMesh = StandardMeshReader.ReadMesh(path);
        readMesh.EnableTriangleGroups();
        readMesh.EnableVertexColors(new Vector3f(1,1,1));
        StaticFunctions.SpawnNewObject(readMesh);

    }
}
