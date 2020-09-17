using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Assets.Classes;
using UnityEngine;

public class Generate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateMesh()
    {
        var foundObjects = FindObjectsOfType<OnMeshClick>();
        foreach (var mesh in foundObjects)
        {
            var subMeshes = new List<Mesh>();
            var meshFilters = mesh.transform.GetComponentsInChildren<MeshFilter>();
            subMeshes.AddRange(meshFilters.Select(meshFilter => meshFilter.sharedMesh));

            var greenTriangles = new List<Triangle>();
            var otherTriangles = new List<Triangle>();
            var allTriangles = new List<Triangle>();
            var allVertices = new Dictionary<Vector3, Vertex>();
            foreach (var (subMesh, subMeshIndex) in subMeshes.LoopIndex())
            {
                int triangleCount = subMesh.triangles.Length;
                var verts = subMesh.vertices;
                var normals = subMesh.normals;
                var colors = subMesh.colors; //all these must be called here, and not time and time again in the loop.
                for (int i = 0; i < triangleCount; i += 3)
                {
                    {
                        var currentTriangle = new Triangle();

                        if (!allVertices.ContainsKey(verts[i])) allVertices.Add(verts[i], new Vertex(verts[i], currentTriangle, i, subMeshIndex));
                        currentTriangle.a = allVertices[verts[i]];
                        allVertices[verts[i]].belongsTo.Add(currentTriangle);

                        if (!allVertices.ContainsKey(verts[i+1])) allVertices.Add(verts[i+1], new Vertex(verts[i+1], currentTriangle, i+1, subMeshIndex));
                        currentTriangle.b = allVertices[verts[i+1]];
                        allVertices[verts[i+1]].belongsTo.Add(currentTriangle);

                        if (!allVertices.ContainsKey(verts[i+2])) allVertices.Add(verts[i+2], new Vertex(verts[i+2], currentTriangle, i+2, subMeshIndex));
                        currentTriangle.c = allVertices[verts[i+2]];
                        allVertices[verts[i+2]].belongsTo.Add(currentTriangle);

                        currentTriangle.n = normals[i];
                        currentTriangle.subMeshNumber = subMeshIndex;
                        currentTriangle.vertexNumberOfA = i;
                        currentTriangle.color = colors[i];
                        allTriangles.Add(currentTriangle);
                    }

                }
            }
            Debug.Log($"Found triangles: {allTriangles.Count}");

            var edgeVertices = new List<Vertex>();
            foreach (var triangle in allTriangles)
            {
                if (triangle.color == Color.green)
                {
                    triangle.CalcDirectNeighbors();
                    if (triangle.abNeighbor.color != Color.green)
                    {
                        edgeVertices.Add(triangle.a);
                        edgeVertices.Add(triangle.b);
                    }

                    if (triangle.bcNeighbor.color != Color.green)
                    {
                        edgeVertices.Add(triangle.b);
                        edgeVertices.Add(triangle.c);
                    }

                    if (triangle.caNeighbor.color != Color.green)
                    {
                        edgeVertices.Add(triangle.c);
                        edgeVertices.Add(triangle.a);
                    }
                }
            }
            for (int i=0; i<edgeVertices.Count-1; i+=2)
            {
                    Debug.DrawLine(mesh.transform.TransformPoint(edgeVertices[i].pos), mesh.transform.TransformPoint(edgeVertices[i+1].pos), Color.red,
                        5, false);
            }
        }
    }

    //private static void MarkOpenEdges(List<Triangle> greenTriangles, OnMeshClick mesh)
    //{
    //    foreach (var firstTriangle in greenTriangles) //if it has already found all their neighbors there cant be any more
    //    {
    //        if (firstTriangle.hasAllNeighbors) continue;
    //        foreach (var secondTriangle in greenTriangles)
    //        {
    //            if (secondTriangle.hasAllNeighbors) continue;
    //            if (secondTriangle.Equals(firstTriangle)) continue;
    //            if (firstTriangle.TryAddAsNeighbor(secondTriangle)
    //            ) //If we could add it to one, we can also add it to the other. If not, we can save the effort.
    //                secondTriangle.TryAddAsNeighbor(firstTriangle);
    //        }
    //    }

    //    var edges = new List<Edge>();
    //    foreach (var greenTriangle in greenTriangles)
    //    {
    //        if (greenTriangle.abNeighbor == null)
    //        {
    //            edges.Add(new Edge()
    //            {
    //                belongsToTriangle = greenTriangle,
    //                side1 = greenTriangle.a,
    //                side2 = greenTriangle.b
    //            });
    //        }

    //        if (greenTriangle.bcNeighbor == null)
    //        {
    //            edges.Add(new Edge()
    //            {
    //                belongsToTriangle = greenTriangle,
    //                side1 = greenTriangle.b,
    //                side2 = greenTriangle.c
    //            });
    //        }

    //        if (greenTriangle.caNeighbor == null)
    //        {
    //            edges.Add(new Edge()
    //            {
    //                belongsToTriangle = greenTriangle,
    //                side1 = greenTriangle.c,
    //                side2 = greenTriangle.a
    //            });
    //        }
    //    }

    //    Debug.Log($"Edges: {edges.Count}");
    //    foreach (var edge in edges)
    //    {
    //        //Debug.DrawLine(edge.side1/20+new Vector3(2.896f,1.54f,-0.279f), edge.side2/20 + new Vector3(2.896f, 1.54f, -0.279f), Color.red, 5, false);
    //        Debug.DrawLine(mesh.transform.TransformPoint(edge.side1), mesh.transform.TransformPoint(edge.side2), Color.red,
    //            5, false);
    //    }
    //}

}
