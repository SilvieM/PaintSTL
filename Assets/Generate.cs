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

                        allVertices.AddIfNotExists(verts[i], new Vertex(verts[i], i, subMeshIndex));
                        currentTriangle.a = allVertices[verts[i]];
                        allVertices[verts[i]].AddBelongsTo(currentTriangle);

                        allVertices.AddIfNotExists(verts[i+1], new Vertex(verts[i+1], i+1, subMeshIndex));
                        currentTriangle.b = allVertices[verts[i+1]];
                        allVertices[verts[i+1]].AddBelongsTo(currentTriangle);

                        allVertices.AddIfNotExists(verts[i + 2], new Vertex(verts[i + 2], i + 3, subMeshIndex));
                        currentTriangle.c = allVertices[verts[i+2]];
                        allVertices[verts[i+2]].AddBelongsTo(currentTriangle);

                        currentTriangle.n = normals[i];
                        currentTriangle.subMeshNumber = subMeshIndex;
                        currentTriangle.vertexNumberOfA = i;
                        currentTriangle.color = colors[i];
                        allTriangles.Add(currentTriangle);
                    }

                }
            }
            Debug.Log($"Found triangles: {allTriangles.Count}");

            var openEdges = new List<Edge>();
            foreach (var triangle in allTriangles)
            {
                if (triangle.color == Color.green)
                {
                    triangle.CalcDirectNeighbors();
                    if (triangle.abNeighbor.color != Color.green)
                    {
                        openEdges.Add(new Edge(triangle.a, triangle.b,
                            new List<Triangle>() {triangle, triangle.abNeighbor}));
                    }

                    if (triangle.bcNeighbor.color != Color.green)
                    {
                        openEdges.Add(new Edge(triangle.b, triangle.c,
                            new List<Triangle>() { triangle, triangle.bcNeighbor }));
                    }

                    if (triangle.caNeighbor.color != Color.green)
                    {
                        openEdges.Add(new Edge(triangle.c, triangle.a,
                            new List<Triangle>() { triangle, triangle.caNeighbor }));
                    }
                }
            }
            foreach (var openEdge in openEdges)
            {
                    Debug.DrawLine(mesh.transform.TransformPoint(openEdge.vertex1.pos), mesh.transform.TransformPoint(openEdge.vertex2.pos), Color.red,
                        5, false);
            }

            var newTriangles = new List<Triangle>();
            foreach (var openEdge in openEdges)
            {
                var dir = (-openEdge.belongsTo[0].n.normalized - openEdge.belongsTo[1].n.normalized).normalized;
                var edgeLength = (openEdge.vertex2.pos - openEdge.vertex1.pos).magnitude;
                Vector3 edgeVector = openEdge.vertex2.pos - openEdge.vertex1.pos;
                var middlePoint = openEdge.vertex1.pos + edgeVector / 2;
                var thirdPoint = middlePoint + dir * edgeLength/2;
                var newTriangle = new Triangle();
                newTriangle.a = openEdge.vertex1;
                newTriangle.b = openEdge.vertex2;
                var newVertex = new Vertex(thirdPoint, 1111, 0);
                allVertices.AddIfNotExists(thirdPoint, newVertex);
                newTriangle.c = newVertex;
                newVertex.AddBelongsTo(newTriangle);
                newTriangles.Add(newTriangle);
            }
            var lastSubmesh = subMeshes.Last();
            var triangles = lastSubmesh.triangles.ToList();
            var vertices = lastSubmesh.vertices.ToList();
            foreach (var newTriangle in newTriangles)
            {
                var index = triangles.Count;
                triangles.AddRange(new List<int>(){index, index+1, index+2});
                vertices.AddRange(new List<Vector3>(){ newTriangle.a.pos, newTriangle.b.pos, newTriangle.c.pos });
            }

            lastSubmesh.triangles = triangles.ToArray();
            lastSubmesh.vertices = vertices.ToArray();

        }
    }


}
