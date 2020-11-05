using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Assets.Classes;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public class Generate : MonoBehaviour
{
    public DMesh3 mesh;

    public DMeshAABBTree3 spatial;

    public int? PointOldPart = null;
    public Vector3d normalMiddle;
    public int? PointNewPart = null;


    public void Start()
    {

    }

    public void Update()
    {
        if (PointOldPart == null || PointNewPart == null) return;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            var tri = mesh.GetVertex((int)PointOldPart);
            mesh.SetVertex((int)PointOldPart, tri + normalMiddle * 0.1);
            g3UnityUtils.SetGOMesh(gameObject, mesh);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            var tri = mesh.GetVertex((int)PointOldPart);
            mesh.SetVertex((int)PointOldPart, tri - normalMiddle * 0.1);
            g3UnityUtils.SetGOMesh(gameObject, mesh);
        }
        //move down
        if (Input.GetKey(KeyCode.RightArrow))
        {

        }
        //move right
        if (Input.GetKey(KeyCode.LeftArrow))
        {

        }
        //move left
    }


    public void MyInit(DMesh3 mesh)
    {
        this.mesh = mesh;
        spatial = new DMeshAABBTree3(mesh);
        spatial.Build();
    }


    public void MakeNewPartPeprAlgo()
    {
        //var paintedTriangles = allTriangles.Where(tri => tri.color!= null &&tri.color==ColorManager.Instance.currentColor).ToList();
        ////somehow because models are usually imported from right coordinate space, they need to be flipped to get correct normals displaying
        //if(!paintedTriangles.Any()) return;
        //var avgNormal = paintedTriangles.Select(tri => tri.n).Average();
        //var newTrianglesInsideFace = paintedTriangles.Select(tri => tri.GetShiftedCopy(avgNormal, allVertices)).ToList();
        //var openEdges = CalcOpenEdges(newTrianglesInsideFace);
        //var newTrianglesSideFaces = new List<Triangle>();
        //foreach (var openEdge in openEdges)
        //{
        //    var correspondingEdge = GetCorrespondingEdge(openEdge);

        //    var triangle1 = new Triangle(allVertices[openEdge.vertex2.pos], allVertices[openEdge.vertex1.pos], allVertices[correspondingEdge.vertex2.pos], ColorManager.Instance.currentColor);
        //    newTrianglesSideFaces.Add(triangle1);
        //    var triangle2 = new Triangle(allVertices[correspondingEdge.vertex2.pos], allVertices[correspondingEdge.vertex1.pos], allVertices[openEdge.vertex2.pos], ColorManager.Instance.currentColor);
        //    newTrianglesSideFaces.Add(triangle2);
        //}

        //paintedTriangles.AddRange(newTrianglesInsideFace);
        //paintedTriangles.AddRange(newTrianglesSideFaces);
        //MakeNewObject(paintedTriangles, avgNormal);

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
        g3UnityUtils.SetGOMesh(gameObject, mesh);
    }

    public List<int> FindPaintedTriangles()
    {
        List<int> indices = new List<int>();
        var colorId = ColorManager.Instance.GetColorId(ColorManager.Instance.currentColor);
        if (colorId != null)
        {
            foreach (var triangleIndex in mesh.TriangleIndices())
            {
                if (mesh.GetTriangleGroup(triangleIndex) == colorId)
                    indices.Add(triangleIndex);
            }


            Debug.Log($"Painted Triangles: {indices.Count}");
        }
        else Debug.Log($"Color Id not found {ColorManager.Instance.currentColor}");

        if (indices.Count <= 0)
        {
            Debug.Log("No colored triangles found");
        }
        return indices;
    }

    /// <summary>
    /// TODO: The one point could be moved by UI controls!
    /// </summary>
    public void MakeNewPartOnePointAlgo()
    {
        var painted = FindPaintedTriangles();
        if (painted.Count <= 0) return;

        painted.Reverse();
        var currentGid = ColorManager.Instance.currentColorId ?? -1;
        var toDelete = new List<int>();
        var newMesh = new DMesh3();
        newMesh.EnableTriangleGroups();
        var normals = new List<Vector3d>();
        var vertices = new List<Vector3d>();
        var verticesInNewMesh = new Dictionary<Vector3d, int>();
        foreach (var paintedTriNum in painted)
        {
            var tri = mesh.GetTriangle(paintedTriNum);
            var normal = mesh.GetTriNormal(paintedTriNum);
            normals.Add(normal);
            var orgA = mesh.GetVertex(tri.a);
            vertices.Add(orgA);
            var orgB = mesh.GetVertex(tri.b);
            vertices.Add(orgB);
            var orgC = mesh.GetVertex(tri.c);
            vertices.Add(orgC);
            var intA = AppendIfNotExists(verticesInNewMesh, orgA, newMesh);
            var intB = AppendIfNotExists(verticesInNewMesh, orgB, newMesh);
            var intC = AppendIfNotExists(verticesInNewMesh, orgC, newMesh);

            var result = mesh.RemoveTriangle(paintedTriNum);
            if (result != MeshResult.Ok) Debug.Log($"Removing did not work, {paintedTriNum} {result}");
            else toDelete.Add(paintedTriNum);
            newMesh.AppendTriangle(intA, intB, intC, currentGid);
        }

        var avgNormal = normals.Average();
        var avgVertices = vertices.Average();
        var newPoint = avgVertices - avgNormal;
        var newPointId = newMesh.AppendVertex(newPoint);
        var newPointIdInOldMesh = mesh.AppendVertex(newPoint);


        var eids = mesh.BoundaryEdgeIndices().ToList();
        MarkEdges(mesh, eids);
        foreach (var openEdge in eids)
        {
            AddTriangle(mesh, openEdge, newPointIdInOldMesh, 0);
        }

        var eidsNewMesh = newMesh.BoundaryEdgeIndices().ToList();
        MarkEdges(newMesh, eidsNewMesh);
        foreach (var openEdge in eidsNewMesh)
        {
            AddTriangle(newMesh, openEdge, newPointId, currentGid);
        }
        mesh = g3UnityUtils.SetGOMesh(gameObject, mesh);
        spatial.Build();
        var newObj = StaticFunctions.SpawnNewObject(newMesh);
        newObj.transform.position += Vector3.forward;
        PointNewPart = newPointId;
        normalMiddle = -avgNormal;
        PointOldPart = newPointIdInOldMesh;
    }

    private void AddTriangle(DMesh3 currentMesh, int openEdge, int centerPoint, int currentGid)
    {
        var edge = currentMesh.GetOrientedBoundaryEdgeV(openEdge);
        currentMesh.AppendTriangle(edge.b, edge.a, centerPoint, currentGid);
    }

    private static int AppendIfNotExists(Dictionary<Vector3d, int> verticesInNewMesh, Vector3d orgA, DMesh3 newMesh)
    {
        if (!verticesInNewMesh.TryGetValue(orgA, out var intA))
        {
            intA = newMesh.AppendVertex(orgA);
            verticesInNewMesh.Add(orgA, intA);
        }
        return intA;
    }

    public void MakeNewPartMyAlgo()
    {
        //var paintedTriangles = allTriangles.Where(tri => tri.color != null && tri.color == ColorManager.Instance.currentColor).ToList();
        //if (!paintedTriangles.Any()) return;
        //var avgNormal = paintedTriangles.Select(tri => tri.n).Average();
        //var lastNumOpenEdges = 0;
        //while (true)
        //{
        //    var openEdges = CalcOpenEdges(paintedTriangles, true);
        //    Debug.Log("Open Edges found: "+openEdges.Count);
        //    if (openEdges.Count == 0||openEdges.Count == lastNumOpenEdges) break;
        //    lastNumOpenEdges = openEdges.Count;
        //    var newTriangles = CreateNewTriangles(openEdges);
        //    paintedTriangles.AddRange(newTriangles);

        //}
        //paintedTriangles.AddRange(paintedTriangles.Select(tri => tri.GetFlippedCopy()).ToList()); //TODO hacky bugfix as now both directions are displayed
        //MakeNewObject(paintedTriangles, avgNormal);
    }

    private void MarkEdges(DMesh3 currentmesh, List<int> eids)
    {

        foreach (var openEdge in eids)
        {
            var edge = currentmesh.GetEdge(openEdge);
            var v0 = currentmesh.GetVertex(edge.a);
            var v1 = currentmesh.GetVertex(edge.b);
            Debug.DrawLine(transform.TransformPoint(v0.toVector3()),
                transform.TransformPoint(v1.toVector3()), Color.red,
                5, false);
        }
    }

    public void DistanceTesting()
    {
        //var triangle = allTriangles.Find(tri => tri.color == ColorManager.Instance.currentColor);
        //if (triangle != null)
        //{
        //    var points = new List<Vector3>()
        //    {
        //        new Vector3(1, 3, 5),
        //        new Vector3(30, 30, 30),
        //        new Vector3(15, 30, 12),
        //        new Vector3(50, 0, 0)
        //    };
        //    foreach (var point in points)
        //    {
        //        var meetPoint = triangle.ClosestPointTo(point);
        //        Debug.DrawLine(transform.TransformPoint(point), transform.TransformPoint(meetPoint), Color.magenta,
        //            5, false);
        //    }
        //}
    }



    public float SquaredDistance(Triangle triangle, Vector3 point)
    {
        var meetPoint = triangle.ClosestPointTo(point);
        return (meetPoint - point).sqrMagnitude;
    }


    private static Mesh MakeNewMeshFromTriangles(IEnumerable<Triangle> newTriangles, int subMeshNumber)
    {
        var trianglesInts1 = new List<int>();
        var vertices1 = new List<Vector3>();
        var colors1 = new List<Color>();
        var normals1 = new List<Vector3>();

        foreach (var triangle in newTriangles)
        {
            var index = trianglesInts1.Count;
            trianglesInts1.AddRange(new List<int>()
                {index + 1, index, index + 2}); //they need to be flipped for some reason
            vertices1.AddRange(new List<Vector3>() { triangle.a.pos, triangle.b.pos, triangle.c.pos });
            colors1.AddRange(new List<Color>() { triangle.color, triangle.color, triangle.color });
            normals1.AddRange(new List<Vector3>() { triangle.n, triangle.n, triangle.n });
        }

        var mesh = new Mesh()
        {
            vertices = vertices1.ToArray(),
            triangles = trianglesInts1.ToArray(),
            colors = colors1.ToArray(),
            normals = normals1.ToArray(),
            name = subMeshNumber.ToString(),
            indexFormat = IndexFormat.UInt16
        };
        return mesh;
    }

    private List<Triangle> CreateNewTriangles(List<Edge> openEdges)
    {
        //var newTriangles = new List<Triangle>();
        //while (openEdges.Any())
        //{
        //    var openEdge = openEdges.First();
        //    openEdges.RemoveAt(0); //basically Dequeue
        //    if (openEdge.belongsTo2 != null) //This is the case for first order triangles: Create new triangle for each free edge
        //    {
        //        var dir = (-openEdge.belongsTo.n.normalized + -openEdge.belongsTo2.n.normalized).normalized;
        //        var edgeLength = openEdge.Delta.magnitude;
        //        var thirdPoint = openEdge.Middlepoint + dir * edgeLength / 2;
        //        var newVertex = new Vertex(thirdPoint, 0, 0);
        //        allVertices.AddIfNotExists(thirdPoint, newVertex);
        //        var newTriangle = new Triangle(openEdge.vertex2, openEdge.vertex1, allVertices[thirdPoint], ColorManager.Instance.currentColor);
        //        newTriangle.color = openEdge.belongsTo.color;
        //        newTriangles.Add(newTriangle);
        //    }
        //    else //This will happen for all non-firstorder triangles: Create new triangle out of 2 existing edges
        //    {
        //        //which side of edge is more connected already
        //        var moreConnectedVertex = openEdge.vertex2.belongsTo.Count > openEdge.vertex1.belongsTo.Count
        //            ? openEdge.vertex2
        //            : openEdge.vertex1;
        //        var lessConnectedVertex = openEdge.vertex1 == moreConnectedVertex ? openEdge.vertex2 : openEdge.vertex1;
        //        //Find second open edge
        //        var brotherEdge = openEdges.FirstOrDefault(edge => edge != openEdge && edge.vertex1 == moreConnectedVertex || edge.vertex2 == moreConnectedVertex) ??
        //                          openEdges.FirstOrDefault(edge => edge != openEdge && edge.vertex1 == lessConnectedVertex || edge.vertex2 == lessConnectedVertex);
        //        if(brotherEdge == null) continue;
        //        //make sure that the edges are not used twice, that is why brother is removed too
        //        openEdges.Remove(brotherEdge);
        //        var openVertexOnBrother = brotherEdge.vertex1 == openEdge.vertex1 ? brotherEdge.vertex2 : brotherEdge.vertex1;
        //        var newTriangle = new Triangle(openEdge.vertex2, openEdge.vertex1, openVertexOnBrother, ColorManager.Instance.currentColor); //TODO respect order!
        //        newTriangle.color = openEdge.belongsTo.color;
        //        newTriangles.Add(newTriangle);
        //    }
        //}

        //return newTriangles;
        return null;
    }


}
