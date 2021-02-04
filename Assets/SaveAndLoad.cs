using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;
using SFB;
using UnityEngine;

public class SaveAndLoad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SavePainted()
    {
        var generates = GameObject.FindObjectsOfType<Generate>();
        var generate = generates.First(gen => gen.isImported);
        string path = StandaloneFileBrowser.SaveFilePanel("Export STL File", "", $"{generate.name}_save ", "");

        if (path.Length == 0)
        {
            return;
        }
        System.IO.Directory.CreateDirectory(path);
        var name = generate.name;
        var mesh = generate.mesh;
        var filename = path + "/" + name + ".g3mesh";
        var options = WriteOptions.Defaults;
        options.bWriteGroups = true;
        options.GroupNameF = i => ColorManager.Instance.GetColorForId(i).ToString(); 
        StandardMeshWriter.WriteFile(filename,
            new List<WriteMesh>() { new WriteMesh(mesh) }, options);
        var colorList = ColorManager.Instance.GetUsedColors().Keys.Select(value => "#"+ ColorUtility.ToHtmlStringRGBA(value)).ToArray();
        System.IO.File.WriteAllLines(path + "/" + name + ".colors", colorList);
    }

    


    public void LoadPainted()
    {
        string[] path = StandaloneFileBrowser.OpenFilePanel("Import STL File", "", "g3mesh", false);
        if (path.Length == 0)
        {
            return;
        }

        DMesh3 readMesh = StandardMeshReader.ReadMesh(path[0]);
        var colorsFile = Path.ChangeExtension(path[0], "colors");
        if (File.Exists(colorsFile))
        {
            ColorManager.Instance.Clear();

            string[] lines = System.IO.File.ReadAllLines(colorsFile);
            var color = UnityEngine.Color.clear;
            foreach (var line in lines)
            {
                ColorUtility.TryParseHtmlString(line, out color);
                ColorManager.Instance.FieldPainted(color);
            }

            ColorManager.Instance.currentColor = color;
        }
        else
        {
            StaticFunctions.ErrorMessage("No color file found.");
        }

        var filename = Path.GetFileNameWithoutExtension(path[0]);
        readMesh.EnableVertexColors(new Vector3f(1, 1, 1));
        readMesh.EnableVertexNormals(Vector3f.One);

        foreach (var vertexIndex in readMesh.VertexIndices())
        {
            readMesh.SetVertexNormal(vertexIndex, readMesh.CalcVertexNormal(vertexIndex).toVector3f());
        }

        var existing = FindObjectsOfType<Generate>();
        foreach (var generate in existing)
        {
            Destroy(generate.gameObject);
        }
        var obj = StaticFunctions.SpawnNewObject(readMesh, filename, true);
        var center = obj.GetComponent<Generate>().centerInWorldCoords;
        obj.transform.position = -center; //position object such that center is zero.
        obj.tag = "mainObject";
        var viewDistance = obj.transform.TransformPoint(readMesh.GetBounds().Extents.toVector3()) * 2;
        Camera.main.GetComponent<OrbitingCam>().SetTarget(obj, viewDistance);

        DebugGizmos.DrawBoundingBox(readMesh.GetBounds(), obj.transform);

    }
}
