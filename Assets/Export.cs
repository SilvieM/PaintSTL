using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;
using SFB;
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
        string path = StandaloneFileBrowser.SaveFilePanel("Export STL File", "","export", "");
        
        if (path.Length == 0)
        {
            return;
        }
        System.IO.Directory.CreateDirectory(path);
        var generates = GameObject.FindObjectsOfType<Generate>();
        var num = 0;
        var name = generates.First(generate => generate.isImported).name;
        foreach (var generate in generates)
        {
            num++;
            StartCoroutine(ExportOneFile(generate, path, name, num));
        }
    }

    private IEnumerator ExportOneFile(Generate generate, string path, string name, int num)
    {
        var mesh = generate.mesh;
        var filename = path + "/"+name + num + ".stl";
        StandardMeshWriter.WriteFile(filename,
            new List<WriteMesh>() {new WriteMesh(mesh)}, WriteOptions.Defaults);
        yield return null;
    }
}
