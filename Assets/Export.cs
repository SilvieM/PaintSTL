using System.Collections;
using System.Collections.Generic;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using UnityTemplateProjects;

public class Export : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ExportSTL()
    {
        string path = EditorUtility.SaveFolderPanel("Export STL File", "","export");
        
        if (path.Length == 0)
        {
            return;
        }

        var generates = GameObject.FindObjectsOfType<Generate>();
        var num = 0;
        foreach (var generate in generates)
        {
            num++;
            StartCoroutine(ExportOneFile(generate, path, num));
        }
        

    }

    private IEnumerator ExportOneFile(Generate generate, string path, int num)
    {
        var mesh = generate.mesh;
        var filename = path + "/export" + num + ".stl";
        StandardMeshWriter.WriteFile(filename,
            new List<WriteMesh>() {new WriteMesh(mesh)}, WriteOptions.Defaults);
        yield return null;
    }
}
