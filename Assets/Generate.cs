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

    public List<Color> colorsPerTri;

    public DMeshAABBTree3 spatial;
    public void Start()
    {

    }


    public void MyInit(DMesh3 mesh)
    {
        this.mesh = mesh;
        this.colorsPerTri = Enumerable.Repeat(Color.white, mesh.TriangleCount).ToList();
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

    

    private static Edge GetCorrespondingEdge(Edge openEdge)
    {
        Edge correspondingEdge;
        if (openEdge.belongsTo.EdgeAb == openEdge)
            correspondingEdge = openEdge.belongsTo.Original.EdgeBc;
        else if (openEdge.belongsTo.EdgeCa == openEdge)
            correspondingEdge = openEdge.belongsTo.Original.EdgeCa;
        else correspondingEdge = openEdge.belongsTo.Original.EdgeAb;
        return correspondingEdge;
    }

    public List<int> FindPaintedTriangles()
    {
        List<int> indices = new List<int>();
        for (int i=0; i < colorsPerTri.Count; i++)
        {
            if (colorsPerTri[i] == ColorManager.Instance.currentColor)
            {
                indices.Add(i);
            }
        }
        return indices;
    }
    public void MakeNewPartOnePointAlgo()
    {
        //DMeshAABBTree3 spatial = new DMeshAABBTree3(mesh);
        //spatial.Build();

        var painted = FindPaintedTriangles();
        painted.Reverse();
        var toDelete = new List<int>();
        var newMesh = new DMesh3();
        foreach (var paintedTriNum in painted)
        {
            var tri = mesh.GetTriangle(paintedTriNum);
            var normal = mesh.GetTriNormal(paintedTriNum);
            var orgA = mesh.GetVertex(tri.a)+normal;
            var orgB = mesh.GetVertex(tri.b)+normal;
            var orgC = mesh.GetVertex(tri.c)+normal;
            var intA = newMesh.AppendVertex(new Vector3d(orgA));
            var intB = newMesh.AppendVertex(new Vector3d(orgB));
            var intC = newMesh.AppendVertex(new Vector3d(orgC));
            
            var result = mesh.RemoveTriangle(paintedTriNum);
            if(result!= MeshResult.Ok) Debug.Log($"Removing did not work, {paintedTriNum} {result}");
            else toDelete.Add(paintedTriNum);
            newMesh.AppendTriangle(intA, intB, intC);
        }
        foreach (var paintedTriNum in toDelete)
        {
            colorsPerTri.RemoveAt(paintedTriNum);
        }
        g3UnityUtils.SetGOMesh(gameObject, mesh, colorsPerTri);
        spatial.Build();
        var newObj = StaticFunctions.SpawnNewObject(newMesh);
        newObj.transform.position += Vector3.forward;
        //var paintedTriangles = allTriangles.Where(tri => tri.color != null && tri.color == ColorManager.Instance.currentColor).ToList();
        //if (!paintedTriangles.Any()) return;
        //var avgNormal = paintedTriangles.Select(tri => tri.n).Average();
        //var middlePointOfSelected = paintedTriangles.Select(tri => tri.middlePoint).Average();
        //var newPoint = middlePointOfSelected - avgNormal *1; //Adjust here for depth
        //allVertices.AddIfNotExists(newPoint, new Vertex(newPoint, 0, 0));
        //var newVertex = allVertices[newPoint];
        //var openEdges = CalcOpenEdges(paintedTriangles, true);
        //var trianglesToDiplay = new List<Triangle>();
        //foreach (var openEdge in openEdges)
        //{
        //    var newTriangle = new Triangle(openEdge.vertex2, openEdge.vertex1, newVertex, ColorManager.Instance.currentColor);
        //    trianglesToDiplay.Add(newTriangle);
        //}

        //CutOutFromMain(trianglesToDiplay, paintedTriangles);

        //trianglesToDiplay.AddRange(paintedTriangles);
        //MakeNewObject(trianglesToDiplay, avgNormal);


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

    private void MarkEdges(List<Edge> edges)
    {
        foreach (var openEdge in edges)
        {
            Debug.DrawLine(transform.TransformPoint(openEdge.vertex1.pos),
                transform.TransformPoint(openEdge.vertex2.pos), Color.red,
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

    private void CutOutFromMain(List<Triangle> add, List<Triangle> remove)
    {
        //foreach (var triangle in remove)
        //{
        //    allTriangles.Remove(triangle);
        //}
        //allTriangles.AddRange(add);
        //RedrawObject();
    }

    private void RedrawObject()
    {
        ////for each previously existing submeshes
        //var subMeshes = new List<Mesh>();
        //var meshFilters = transform.GetComponentsInChildren<MeshFilter>();
        //subMeshes.AddRange(meshFilters.Select(meshFilter => meshFilter.sharedMesh));
        //for (int i=0; i<subMeshes.Count;i++)
        //{
        //    var trianglesToPutHere = allTriangles.Where(tri => tri.subMeshNumber == i&&tri.isGenerated==false);

        //    subMeshes[i] = MakeNewMeshFromTriangles(trianglesToPutHere, i);

        //}
        ////one new submesh for all newly generated triangles
        //var newTriangles = allTriangles.Where(tri => tri.isGenerated == true);
        //var mesh = MakeNewMeshFromTriangles(newTriangles, subMeshes.Count);

        //var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //Object.DestroyImmediate(go.GetComponent<BoxCollider>());
        //go.transform.SetParent(transform, false);
        //go.name = name + "(" + subMeshes.Count + ")";
        //go.GetComponent<MeshFilter>().sharedMesh = mesh;
        //var res = Resources.Load("STLMeshMaterial2") as Material;
        //go.GetComponent<MeshRenderer>().material = res;
        //go.AddComponent<MeshCollider>();
        //go.transform.position = Vector3.zero;

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
            vertices1.AddRange(new List<Vector3>() {triangle.a.pos, triangle.b.pos, triangle.c.pos});
            colors1.AddRange(new List<Color>() {triangle.color, triangle.color, triangle.color});
            normals1.AddRange(new List<Vector3>() {triangle.n, triangle.n, triangle.n});
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


    private List<Edge> CalcOpenEdges(List<Triangle> triangles, bool colorFilter = false)
    {
        var openEdges = new List<Edge>();
        foreach (var triangle in triangles)
        {
            if (!colorFilter || triangle.color == ColorManager.Instance.currentColor)
            {
                triangle.CalcDirectNeighbors();
                if (triangle.abNeighbor == null || triangle.abNeighbor.color != triangle.color)
                {
                    openEdges.Add(triangle.EdgeAb);
                }

                if (triangle.bcNeighbor == null || triangle.bcNeighbor.color != triangle.color)
                {
                    openEdges.Add(triangle.EdgeBc);
                }

                if (triangle.caNeighbor == null || triangle.caNeighbor.color != triangle.color)
                {
                    openEdges.Add(triangle.EdgeCa);
                }
            }
        }

        return openEdges;
    }

    public void MakeNewObject(List<Triangle> triangles, Vector3 offset)
    {
        var mesh = MakeNewMeshFromTriangles(triangles, 0);
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(go.GetComponent<BoxCollider>());
        go.name = "SplitObject";
        go.GetComponent<MeshFilter>().sharedMesh = mesh;
        var res = Resources.Load("STLMeshMaterial2") as Material;
        go.GetComponent<MeshRenderer>().material = res;
        //go.AddComponent<OnMeshClick>().Start();
        //go.AddComponent<Generate>().GenerateMesh(); //Possibly can make the new splitted objects splittable too?
        go.AddComponent<MeshCollider>();
        go.transform.position = offset;
        go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    public void DisplayNormals(List<Triangle> triangles = null)
    {
        //if (triangles == null) triangles = allTriangles;
        //foreach (var triangle in triangles)
        //{
        //    var mid = (triangle.a.pos + triangle.b.pos + triangle.c.pos) / 3;
        //    DrawArrow.LineForDebug(transform.TransformPoint(mid), transform.TransformPoint(mid + triangle.n.normalized), Color.cyan);
        //}
    }



}
