using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Assets.Classes;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public class Generate : MonoBehaviour
{
    //TODO move the functionality on each object, and only call the methods from here.
    public Dictionary<(Vector3, Vector3), Edge> allEdges { get; set; }

    public Dictionary<Vector3, Vertex> allVertices { get; set; }

    public List<Triangle> allTriangles { get; set; }

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
        var subMeshes = new List<Mesh>();
        var meshFilters = transform.GetComponentsInChildren<MeshFilter>();
        subMeshes.AddRange(meshFilters.Select(meshFilter => meshFilter.sharedMesh));

        allTriangles = new List<Triangle>();
        allVertices = new Dictionary<Vector3, Vertex>();
        allEdges = new Dictionary<(Vector3, Vector3), Edge>();
        foreach (var (subMesh, subMeshIndex) in subMeshes.LoopIndex())
        {
            int triangleCount = subMesh.triangles.Length;
            var verts = subMesh.vertices;
            var normals = subMesh.normals;
            var colors = subMesh.colors; //all these must be called here, and not time and time again in the loop.
            for (int i = 0; i < triangleCount; i += 3)
            {
                {
                    allVertices.AddIfNotExists(verts[i], new Vertex(verts[i], i, subMeshIndex));

                    allVertices.AddIfNotExists(verts[i + 1], new Vertex(verts[i + 1], i + 1, subMeshIndex));

                    allVertices.AddIfNotExists(verts[i + 2], new Vertex(verts[i + 2], i + 3, subMeshIndex));

                    var currentTriangle = new Triangle(allVertices[verts[i]], allVertices[verts[i + 1]], allVertices[verts[i + 2]], normals[i]);

                    currentTriangle.subMeshNumber = subMeshIndex;
                    currentTriangle.vertexNumberOfA = i;
                    currentTriangle.color = colors[i];

                    allTriangles.Add(currentTriangle);
                }

            }
        }
        Debug.Log($"Found triangles: {allTriangles.Count}");
        var allNewTriangles = new List<Triangle>();
        for (int i = 0; i <= 4; i++)
        {
            var openEdges = CalcOpenEdges(i<0);
            var newTriangles = CreateNewTriangles(openEdges);
            allNewTriangles.AddRange(newTriangles);
            allTriangles.AddRange(newTriangles);
        }
        
        DrawNewTriangles(subMeshes, allNewTriangles);
        var openEdgesToDraw = CalcOpenEdges();
        MarkOpenEdges(openEdgesToDraw);
    }

    private void MarkOpenEdges(List<Edge> openEdges)
    {
        foreach (var openEdge in openEdges)
        {
            Debug.DrawLine(transform.TransformPoint(openEdge.vertex1.pos),
                transform.TransformPoint(openEdge.vertex2.pos), Color.red,
                5, false);
        }
    }

    public void DistanceTesting()
    {
        var triangle = allTriangles.Find(tri => tri.color == Color.green);
        if (triangle != null)
        {
            var points = new List<Vector3>()
            {
                new Vector3(1, 3, 5),
                new Vector3(30, 30, 30),
                new Vector3(15, 30, 12),
                new Vector3(50, 0, 0)
            };
            foreach (var point in points)
            {
                var meetPoint = triangle.ClosestPointTo(point);
                Debug.DrawLine(transform.TransformPoint(point), transform.TransformPoint(meetPoint), Color.magenta,
                    5, false);
            }
        }
    }



    public float SquaredDistance(Triangle triangle, Vector3 point)
    {
        var meetPoint = triangle.ClosestPointTo(point);
        return (meetPoint - point).sqrMagnitude;
    }


    private List<Triangle> CreateNewTriangles(List<Edge> openEdges)
    {
        var newTriangles = new List<Triangle>();
        foreach (var openEdge in openEdges)
        {
            if (openEdge.belongsTo2 != null) //This is the case for first order triangles: Create new triangle for each free edge
            {
                var dir = (-openEdge.belongsTo.n.normalized - openEdge.belongsTo2.n.normalized).normalized;
                var edgeLength = (openEdge.vertex2.pos - openEdge.vertex1.pos).magnitude;
                Vector3 edgeVector = openEdge.vertex2.pos - openEdge.vertex1.pos;
                var middlePoint = openEdge.vertex1.pos + edgeVector / 2;
                var thirdPoint = middlePoint + dir * edgeLength / 2;
                var newVertex = new Vertex(thirdPoint, 1111, 0);
                allVertices.AddIfNotExists(thirdPoint, newVertex);
                var newTriangle = new Triangle(openEdge.vertex1, openEdge.vertex2, allVertices[thirdPoint], Vector3.zero);//TODO  calculate normals properly!
                newTriangle.color = openEdge.belongsTo.color;
                newTriangles.Add(newTriangle);
            }
            else //This will happen for all non-firstorder triangles: Create new triangle out of 2 existing edges
            {
                //TODO make sure that the edges are not used twice!!
                //Find second open edge
                var brotherEdge = openEdges.First(edge => edge != openEdge && edge.vertex1 == openEdge.vertex1 || edge.vertex2 == openEdge.vertex1);
                if (brotherEdge == null) continue;
                var openVertexOnBrother = brotherEdge.vertex1 == openEdge.vertex1 ? brotherEdge.vertex2 : brotherEdge.vertex1;
                var newTriangle = new Triangle(openEdge.vertex1, openEdge.vertex2, openVertexOnBrother,
                    Vector3.zero);
                newTriangle.color = openEdge.belongsTo.color;
                newTriangles.Add(newTriangle);
            }
        }

        return newTriangles;
    }

    //TODO also draw new normals
    public void DrawNewTriangles(List<Mesh> subMeshes, List<Triangle> allnewTriangles)
    {
        var lastSubmesh = subMeshes.Last();
        var triangles = lastSubmesh.triangles.ToList();
        var vertices = lastSubmesh.vertices.ToList();
        var colors = lastSubmesh.colors.ToList();
        foreach (var newTriangle in allnewTriangles)
        {
            var index = triangles.Count;
            triangles.AddRange(new List<int>() { index, index + 1, index + 2 });
            vertices.AddRange(new List<Vector3>() { newTriangle.a.pos, newTriangle.b.pos, newTriangle.c.pos });
            colors.AddRange(new List<Color>() {newTriangle.color, newTriangle.color, newTriangle.color});
        }

        lastSubmesh.vertices = vertices.ToArray();
        lastSubmesh.triangles = triangles.ToArray();
        lastSubmesh.colors = colors.ToArray();
    }

    private List<Edge> CalcOpenEdges(bool firstpass = false)
    {
        var openEdges = new List<Edge>();
        foreach (var triangle in allTriangles)
        {
            if (triangle.color == Color.green)
            {
                triangle.CalcDirectNeighbors();
                if (triangle.abNeighbor == null || triangle.abNeighbor.color != Color.green)
                {
                    openEdges.Add(triangle.EdgeAb);
                }

                if (triangle.bcNeighbor == null || triangle.bcNeighbor.color != Color.green)
                {
                    openEdges.Add(triangle.EdgeBc);
                }

                if (triangle.caNeighbor == null || triangle.caNeighbor.color != Color.green)
                {
                    openEdges.Add(triangle.EdgeCa);
                }
            }
        }

        return openEdges;
    }

    //TODO remove splitted stuff from original model
    public void Split()
    {
        if(allTriangles == null) return;
        var triangles = new List<int>();
        var vertices = new List<Vector3>();
        var colors = new List<Color>();
        var normals = new List<Vector3>();
        foreach (var triangle in allTriangles)
        {
            if (triangle.color != Color.green) continue;
            var index = triangles.Count;
            triangles.AddRange(new List<int>() {index, index + 1, index + 2});
            vertices.AddRange(new List<Vector3>() {triangle.a.pos, triangle.b.pos, triangle.c.pos});
            colors.AddRange(new List<Color>() {triangle.color, triangle.color, triangle.color});
            normals.AddRange(new List<Vector3>(){triangle.n, triangle.n, triangle.n});
        }
        var mesh = new Mesh()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            colors = colors.ToArray(),
            normals = normals.ToArray(),
            name = "SplitObjectMesh",
            indexFormat = IndexFormat.UInt16
        };
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(go.GetComponent<BoxCollider>());
        go.name = "SplitObject";
        go.GetComponent<MeshFilter>().sharedMesh = mesh;
        var res = Resources.Load("SeeThruSTLMeshMaterial") as Material;
        go.GetComponent<MeshRenderer>().material = res;
        go.AddComponent<OnMeshClick>();
        go.AddComponent<Generate>();
        go.AddComponent<MeshCollider>();
        go.transform.position = -Vector3.one;
        go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        
    }



}
