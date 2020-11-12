using System.Collections.Generic;
using System.Linq;
using Assets;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;
using UnityEngine;

public class Generate : MonoBehaviour
{
    public DMesh3 mesh;

    public DMeshAABBTree3 spatial;

    public int? PointOldPart = null;
    public Vector3d normalMiddle;
    public int? ThisObjectsPoint = null;

    public void Start()
    {

    }

    public void Update()
    {
        var PointsToMove = mesh.VertexIndices().Where(index =>
            mesh.GetVertexColor(index) == ColorManager.Instance.currentColor.toVector3f()); //Ressource-hungry??
        foreach (var PointToMove in PointsToMove)
        {
            var normal = mesh.GetVertexNormal(PointToMove);
            if (Input.GetKey(KeyCode.UpArrow))
            {
                var tri = mesh.GetVertex(PointToMove);
                mesh.SetVertex(PointToMove, tri + normal.toVector3d() * 0.1);
                g3UnityUtils.SetGOMesh(gameObject, mesh);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                var tri = mesh.GetVertex(PointToMove);
                mesh.SetVertex(PointToMove, tri - normal.toVector3d() * 0.1);
                g3UnityUtils.SetGOMesh(gameObject, mesh);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                var tri = mesh.GetVertex(PointToMove);
                mesh.SetVertex(PointToMove, tri + Camera.main.transform.right.toVector3d() * 0.1);
                g3UnityUtils.SetGOMesh(gameObject, mesh);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                var tri = mesh.GetVertex(PointToMove);
                mesh.SetVertex(PointToMove, tri - Camera.main.transform.right.toVector3d() * 0.1);
                g3UnityUtils.SetGOMesh(gameObject, mesh);
            }
        }
    }


    public void MyInit(DMesh3 mesh)
    {
        this.mesh = mesh;
        spatial = new DMeshAABBTree3(mesh);
        spatial.Build();
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

    public void MakeNewPartOnePointAlgo()
    {
        var painted = FindPaintedTriangles();
        if (painted.Count <= 0) return;

        painted.Reverse();
        var currentGid = ColorManager.Instance.currentColorId ?? -1;
        var toDelete = new List<int>();
        var newMesh = new DMesh3();
        newMesh.EnableTriangleGroups();
        newMesh.EnableVertexColors(new Vector3f(1,1,1));
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
        mesh.SetVertexColor(newPointIdInOldMesh, ColorManager.Instance.currentColor.toVector3f());
        mesh = g3UnityUtils.SetGOMesh(gameObject, mesh);
        spatial.Build();
        newMesh.SetVertexColor(newPointId, ColorManager.Instance.currentColor.toVector3f());
        var newObj = StaticFunctions.SpawnNewObject(newMesh);
        newObj.transform.position += Vector3.forward;
        newObj.GetComponent<Generate>().ThisObjectsPoint = newPointId;
        newObj.GetComponent<Generate>().normalMiddle = -avgNormal;
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
}
