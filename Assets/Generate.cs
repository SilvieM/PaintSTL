using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Assets;
using Assets.Algorithms;
using Assets.Classes;
using Assets.g3UnityUtils;
using g3;
using gs;
using UnityEngine;

public class Generate : MonoBehaviour
{
    public DMesh3 mesh;
    public DMeshAABBTree3 spatial;
    public bool isImported;
    public DMesh3 originalMesh;
    public Vector3 center;
    public Vector3 centerInWorldCoords => transform.TransformPoint(center);
    public CuttingInfo cuttingInfo; //if this is a cut base, then the info about cut is here
    public void Start()
    {

    }


    public void Update()
    {
        //if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.Z))
        //{
        //    Debug.Log("Undo");
        //    var action = StateManager.Instance.PeekLastAction();
        //    if (action == null) return;
        //    Debug.Log($"Undo {action.painted.Count}, {action.painted.Values.First()}");
        //    foreach (var keyValuePair in action.painted)
        //    {
        //        mesh.SetTriangleGroup(keyValuePair.Key, keyValuePair.Value);
        //    }
        //    StateManager.Instance.CommitLastAction();
        //}
        //Redraw();
    }


    public void MyInit(DMesh3 mesh, bool isImported = false)
    {
        this.mesh = mesh;
        spatial = new DMeshAABBTree3(mesh);
        spatial.Build();
        this.isImported = isImported;
        if (isImported)
        {
            originalMesh = new DMesh3(mesh);
        }
        center = transform.TransformPoint(mesh.GetBounds().Center.toVector3());

    }

    public void Remesh()
    {
        //Remesher r = new Remesher(mesh);
        //r.PreventNormalFlips = true;
        //double min_edge_len, max_edge_len, avg_edge_len;
        //MeshQueries.EdgeLengthStats(mesh, out min_edge_len, out max_edge_len, out avg_edge_len);
        //r.SetTargetEdgeLength(avg_edge_len * 0.8);
        //r.SetProjectionTarget(MeshProjectionTarget.Auto(mesh));
        //r.BasicRemeshPass();
        
        Reducer r = new Reducer(mesh);
        r.ReduceToTriangleCount(mesh.TriangleCount/2);
        foreach (var vertexIndex in mesh.VertexIndices())
        {
            mesh.SetVertexNormal(vertexIndex, mesh.CalcVertexNormal(vertexIndex).toVector3f());
        }
        Redraw(true);
    }


    public void Cut(List<CutSettingData> cutSettings)
    {
        foreach (var cutSettingData in cutSettings)
        {
            StartCoroutine(CutCoroutine(cutSettingData));
        }

        
    }

    private IEnumerator CutCoroutine(CutSettingData cutSettings)
    {
        var algorithm = Algorithm.BuildAlgo(cutSettings.algo);
        var info = new CuttingInfo()
        {
            mesh = mesh,
            oldMesh = originalMesh,
            data = cutSettings
        };
        var newMesh = algorithm.Cut(info);
        mesh = g3UnityUtils.SetGOMesh(gameObject, newMesh);
        yield return null;
    }

    public void FixMyPaintJob()
    {
        foreach (var triIndex in mesh.TriangleIndices())
        {
            var thisTriGroup = mesh.GetTriangleGroup(triIndex);
            var neighbors = mesh.GetTriNeighbourTris(triIndex);
            var triGroup1 = mesh.GetTriangleGroup(neighbors[0]);
            if (triGroup1 == thisTriGroup) continue;
            var triGroup2 = mesh.GetTriangleGroup(neighbors[1]);

            if (triGroup2 == thisTriGroup) continue;
            var triGroup3 = mesh.GetTriangleGroup(neighbors[2]);
            if (triGroup1 == triGroup2 && triGroup2 == triGroup3)
            {
                mesh.SetTriangleGroup(triIndex, triGroup1);
                var colors = gameObject.GetComponent<MeshFilter>().sharedMesh.colors;
                colors[triIndex] = ColorManager.Instance.GetColorForId(triGroup1);
                gameObject.GetComponent<MeshFilter>().sharedMesh.colors = colors;
            }

        }
        Redraw();
    }

    public void Redraw(bool doCompact = false)
    {
        mesh = g3UnityUtils.SetGOMesh(gameObject, mesh, null, doCompact);
    }

    public void Explode(float value)
    {
        if (!isImported)
        {
            if (value > 0)
            {
                var mainObject = FindObjectsOfType<Generate>().First(gen => gen.isImported);
                var dir = (this.center - mainObject.center).normalized;
                transform.localPosition = dir*value*30;
            }
            else
            {
                transform.localPosition = Vector3.zero;
            }
        }
       
    }

    public List<List<int>> FindGroups(List<int> paintedTriangles)
    {
        var allGroups = new List<List<int>>();
        while (paintedTriangles.Any())
        {
            var group = new List<int>() { paintedTriangles.First() };
            if(paintedTriangles.Count>1)
                for (var index = 1; index < paintedTriangles.Count; index++) //start with second element
                {
                    var paintedTriangle = paintedTriangles[index];
                    var neigborsOfGroup = group.SelectMany(tri => mesh.GetTriNeighbourTris(tri).array);
                    if (neigborsOfGroup.Contains(paintedTriangle))
                    {
                        group.Add(paintedTriangle);
                    }
                }
            paintedTriangles.RemoveAll(index => group.Contains(index)); //Remove found ones
            //Second pass
            if (paintedTriangles.Count > 1)
                for (var index = 1; index < paintedTriangles.Count; index++) //start with second element
                {
                    var paintedTriangle = paintedTriangles[index];
                    var neigborsOfGroup = group.SelectMany(tri => mesh.GetTriNeighbourTris(tri).array);
                    if (neigborsOfGroup.Contains(paintedTriangle))
                    {
                        group.Add(paintedTriangle);
                    }
                }
            allGroups.Add(group);
        }

        return allGroups;
    }

    public int SanityCheck()
    {
        if (isImported) return 0;
        var tree = new DMeshAABBTree3(cuttingInfo.oldMesh, true); //todo build tree earlier??
        var errors = 0;
        foreach (var keyValuePair in cuttingInfo.PointToPoint)
        {
            if(!mesh.IsVertex(keyValuePair.Key)) Debug.Log($"NonVertex from {cuttingInfo.data.algo.ToString()}");
            var coords = mesh.GetVertex(keyValuePair.Key);
            var oldMesh = cuttingInfo.oldMesh;

            if (!tree.IsInside(coords)) errors++;
        }

        return errors;
    }
    

    public void SaveColored()
    {
        if (isImported)
        {
            originalMesh.Copy(mesh);
        }
    }

    public void Revert()
    {
        if (isImported)
        {
            this.mesh = new DMesh3(originalMesh);
            g3UnityUtils.SetGOMesh(gameObject, mesh);
        }
        else Destroy(gameObject);
    }

    
}
