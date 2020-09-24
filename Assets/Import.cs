using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Parabox.Stl;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using Object = UnityEngine.Object;

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
        Debug.Log("Import Clicked");
        string path = EditorUtility.OpenFilePanel("Import STL File", "", "stl");
        GameObject created = null;
        if (path.Length == 0)
        {
            return;
        }

        var name = Path.GetFileNameWithoutExtension(path);
        var meshes = Importer.Import(path, CoordinateSpace.Right).ToArray(); //TODO Check Coordinate Space

        if (meshes.Length < 1)
            return;

        if (meshes.Length < 2)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(go.GetComponent<BoxCollider>());
            go.name = name;
            meshes[0].name = "Mesh-" + name;
            go.GetComponent<MeshFilter>().sharedMesh = meshes[0];
            var res = Resources.Load("STLMeshMaterial2") as Material;
            go.GetComponent<MeshRenderer>().material = res;
            go.AddComponent<OnMeshClick>();
            go.AddComponent<MeshCollider>();
            go.transform.position = Vector3.zero;
            go.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
            created = go;
        }
        else
        {
            var parent = new GameObject();
            parent.name = name;
            for (int i = 0, c = meshes.Length; i < c; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(go.GetComponent<BoxCollider>());
                go.transform.SetParent(parent.transform, false);
                go.name = name + "(" + i + ")";

                var mesh = meshes[i];
                mesh.name = "Mesh-" + name + "(" + i + ")";
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
                var res = Resources.Load("STLMeshMaterial2") as Material;
                go.GetComponent<MeshRenderer>().material = res;
                go.AddComponent<MeshCollider>();
                go.transform.position = Vector3.zero;
            }
            parent.AddComponent<OnMeshClick>();
            parent.transform.position = Vector3.zero;
            parent.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            created = parent;
        }
        var cam = GameObject.FindGameObjectWithTag("MainCamera");
        cam.transform.LookAt(created.transform);
        
    }
}
