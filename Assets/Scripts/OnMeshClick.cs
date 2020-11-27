using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Assets.g3UnityUtils;
using g3;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

public class OnMeshClick : MonoBehaviour
{
    public double range = 1f;
    // Start is called before the first frame update
    public void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100) && (hit.transform == transform || hit.transform.parent == transform))
        {
            var paintColor = ColorManager.Instance.currentColor;
            var mesh = hit.transform.gameObject.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] meshtriangles = mesh.triangles;
            Vector3 p0 = vertices[meshtriangles[hit.triangleIndex * 3 + 0]];
            Vector3 p1 = vertices[meshtriangles[hit.triangleIndex * 3 + 1]];
            Vector3 p2 = vertices[meshtriangles[hit.triangleIndex * 3 + 2]];
            p0 = hit.transform.TransformPoint(p0);
            p1 = hit.transform.TransformPoint(p1);
            p2 = hit.transform.TransformPoint(p2);
            Debug.DrawLine(p0, p1, ColorManager.Instance.currentColor);
            Debug.DrawLine(p1, p2, ColorManager.Instance.currentColor);
            Debug.DrawLine(p2, p0, ColorManager.Instance.currentColor);

            if (Input.GetMouseButton(0))
            {
                var triIndices = new List<int>();
                var generate = GetComponent<Generate>();
                triIndices.Add(hit.triangleIndex);
                var dmesh = generate.mesh;
                bool foundNewTriangle = true;
                while (foundNewTriangle)
                {
                    foundNewTriangle = false;
                    var newTriIndices = new List<int>();
                    foreach (var triIndexAlreadyFound in triIndices)
                    {
                        var neighborTris = dmesh.GetTriNeighbourTris(triIndexAlreadyFound);

                        foreach (var triIndex in neighborTris.array)
                        {
                            if (!triIndices.Contains(triIndex)&&!newTriIndices.Contains(triIndex) && IsInRange(dmesh, hit.triangleIndex, triIndex, range))
                            {
                                newTriIndices.Add(triIndex);
                                foundNewTriangle = true;
                            }
                        }
                    }
                    triIndices.AddRange(newTriIndices);
                    
                }

                var colorIndex = ColorManager.Instance.FieldPainted(paintColor);

                var colorsNew = mesh.colors;

                foreach (var triIndex in triIndices)
                {
                    dmesh.SetTriangleGroup(triIndex, colorIndex);
                    for (int i = 0; i < 3; i++)
                    {
                        colorsNew[meshtriangles[triIndex * 3 + i]] = paintColor;
                    }
                }
                mesh.colors = colorsNew;

            }
        }
    }

    private bool IsInRange(DMesh3 mesh,int triIndexOriginal, int triIndex, double range)
    {
        var triOriginal = mesh.GetTriCentroid(triIndexOriginal);
        var tri = mesh.GetTriangle(triIndex);
        var v1 = mesh.GetVertex(tri.a);
        var v2 = mesh.GetVertex(tri.b);
        var v3 = mesh.GetVertex(tri.c);
        var rangeSquared = Math.Pow(range, 2);
        if (v1.DistanceSquared(triOriginal) < rangeSquared) return true;
        if (v2.DistanceSquared(triOriginal) < rangeSquared) return true;
        if (v3.DistanceSquared(triOriginal) < rangeSquared) return true;
        return false;
    }

}
