﻿using System.Collections;
using System.IO;
using System.Linq;
using Assets;
using Assets.g3UnityUtils;
using Assets.Scripts;
using Assets.Static_Classes;
using g3;
using gs;
using SFB;
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
        string[] path = StandaloneFileBrowser.OpenFilePanel("Import STL File", "", "stl", false);
        if (path.Length == 0)
        {
            return;
        }
        var coroutine = ReadMesh(path[0]);
        StartCoroutine(coroutine);
        ColorManager.Instance.currentColor = Color.white;
        ColorManager.Instance.FieldPainted(Color.white);
        ColorManager.Instance.currentColor = Color.red;
    }

    private IEnumerator ReadMesh(string path)
    {
        DMesh3 readMesh = StandardMeshReader.ReadMesh(path);
        var errorMsg = "";
        if (!readMesh.CheckValidity(eFailMode: FailMode.ReturnOnly))
        {
            errorMsg += "Imported Model has errors.";
            var loops = new MeshBoundaryLoops(readMesh, true);
            if (loops.SawOpenSpans) errorMsg += " Open Spans can not be filled. ";
            var fixedHoles = 0;
            foreach (var meshBoundaryLoop in loops)
            {
                var holeFiller = new SimpleHoleFiller(readMesh, meshBoundaryLoop);
                var valid = holeFiller.Validate();
                if (valid == ValidationStatus.Ok)
                {
                    var res = holeFiller.Fill(0);
                    if (res) fixedHoles++;
                }
            }

            if (fixedHoles > 0) errorMsg += $"Fixed {fixedHoles} holes for you. ";
            if (!loops.Any()) errorMsg += "No holes had to be fixed. ";
            MeshAutoRepair repair = new MeshAutoRepair(readMesh);
            bool bOK = repair.Apply();
            if (bOK == false)
                errorMsg += ("Autorepair failed. ");
            else
                errorMsg += ("Autorepair done. ");
            if (readMesh.CheckValidity(eFailMode: FailMode.ReturnOnly)) errorMsg += "Model is now valid. ";
            else errorMsg += "Could not fix all errors.";
        }

        //var occlusionRemover = new RemoveOccludedTriangles(readMesh);
        //bool changed = occlusionRemover.Apply();
        //if (changed) errorMsg += " Removed occluded triangles.";

        if(errorMsg != "") StaticFunctions.ErrorMessage(errorMsg);
        Debug.Log(errorMsg);
        if (!readMesh.IsCompact)
        {
            readMesh = new DMesh3(readMesh, true);
        }
        
        var filename = Path.GetFileNameWithoutExtension(path);
        readMesh.EnableTriangleGroups();
        readMesh.EnableVertexColors(new Vector3f(1, 1, 1));
        readMesh.EnableVertexNormals(Vector3f.One);
        //MeshNormals.QuickCompute(mesh: readMesh);
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
        var angleSlider = FindObjectOfType<AngleSlider>();
        if (angleSlider != null) angleSlider.OnSliderWasChanged();
        var brushSizeSlider = FindObjectOfType<BrushSizeSlider>();
        if (brushSizeSlider != null) brushSizeSlider.OnSliderWasChanged();
        yield return null;
    }
}
