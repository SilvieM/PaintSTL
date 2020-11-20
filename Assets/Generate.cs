using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Assets;
using Assets.g3UnityUtils;
using Assets.Static_Classes;
using g3;
using UnityEngine;

public class Generate : MonoBehaviour
{
    public DMesh3 mesh;
    public DMesh3 originalMesh;
    public DMeshAABBTree3 spatial;

    public int? PointOldPart = null;
    public Vector3d normalMiddle;
    public int? ThisObjectsPoint = null;

    public void Start()
    {

    }

    public void Update()
    {
        if (!(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow) ||
              Input.GetKey(KeyCode.LeftArrow))) return;
        var PointsToMove = mesh.VertexIndices().Where(index =>
            mesh.GetVertexColor(index) == ColorManager.Instance.currentColor.toVector3f()); //Ressource-hungry??
        foreach (var PointToMove in PointsToMove)
        {
            var normal = mesh.GetVertexNormal(PointToMove);
            var tri = mesh.GetVertex(PointToMove);
            var newPos= tri;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                newPos = tri + normal * 0.1f;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                newPos = tri - normal * 0.1f;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                newPos = tri + Camera.main.transform.right.toVector3d() * 0.1;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                newPos = tri - Camera.main.transform.right.toVector3d() * 0.1;
            }
            if (CheckPositionValid(originalMesh, newPos)) mesh.SetVertex(PointToMove, newPos);
        }
        g3UnityUtils.SetGOMesh(gameObject, mesh);
    }


    public void MyInit(DMesh3 mesh, DMesh3 originalMesh = null)
    {
        this.mesh = mesh;
        this.originalMesh = originalMesh ?? new DMesh3(this.mesh);
        spatial = new DMeshAABBTree3(originalMesh);
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

    public void MakeNewPartOnePointAlgo()
    {
        var painted = FindPaintedTriangles();
        if (painted.Count <= 0) return;

        //var groups = FindGroups(painted);
        //Debug.Log($"Groups: {groups.Count}");

        //foreach (var group in groups)
        //{
            painted.Reverse();
            var currentGid = ColorManager.Instance.currentColorId ?? -1;
            var toDelete = new List<int>();
            var newMesh = new DMesh3();
            newMesh.EnableTriangleGroups();
            newMesh.EnableVertexColors(new Vector3f(1, 1, 1));
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
            
            spatial.Build();
            newMesh.SetVertexColor(newPointId, ColorManager.Instance.currentColor.toVector3f());
            var newObj = StaticFunctions.SpawnNewObject(newMesh, originalMesh);
            newObj.transform.position += Vector3.forward;
            newObj.GetComponent<Generate>().ThisObjectsPoint = newPointId;
            newObj.GetComponent<Generate>().normalMiddle = -avgNormal;
            normalMiddle = -avgNormal;
            PointOldPart = newPointIdInOldMesh;
        //}
        mesh = g3UnityUtils.SetGOMesh(gameObject, mesh);


    }

    private void AddTriangle(DMesh3 currentMesh, int openEdge, int centerPoint, int currentGid)
    {
        var edge = currentMesh.GetOrientedBoundaryEdgeV(openEdge);
        currentMesh.AppendTriangle(edge.b, edge.a, centerPoint, currentGid);
    }

    private static int AppendIfNotExists(Dictionary<Vector3d, int> verticesInNewMesh, Vector3d position, DMesh3 newMesh)
    {
        if (!verticesInNewMesh.TryGetValue(position, out var intA))
        {
            intA = newMesh.AppendVertex(position);
            verticesInNewMesh.Add(position, intA);
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

    public void CutPeprAlgo()
    {
        var painted = FindPaintedTriangles();
        if (painted.Count <= 0) return;

        var newMesh = new DMesh3();
        newMesh.EnableTriangleGroups();
        newMesh.EnableVertexColors(new Vector3f(1, 1, 1));

        var verticesInNewMesh = new Dictionary<Vector3d, int>();
        var verticesInOldMesh = new Dictionary<Vector3d, int>();
        var InnerToOuter = new Dictionary<int, int>();
        var InnerToOuterOldMesh = new Dictionary<int, int>();
        foreach (var triIndex in painted)
        {
            var triangle = mesh.GetTriangle(triIndex);
            var vertex1 = mesh.GetVertex(triangle.a);
            var vertex2 = mesh.GetVertex(triangle.b);
            var vertex3 = mesh.GetVertex(triangle.c);

            var intAOuter = AppendIfNotExists(verticesInNewMesh, vertex1, newMesh);
            var intBOuter = AppendIfNotExists(verticesInNewMesh, vertex2, newMesh); 
            var intCOuter = AppendIfNotExists(verticesInNewMesh, vertex3, newMesh);

            var newTriOuter = newMesh.AppendTriangle(intAOuter, intBOuter, intCOuter);

            var normal = mesh.GetTriNormal(triIndex);
            var normal1 = mesh.CalcVertexNormal(triangle.a);
            var normal2 = mesh.CalcVertexNormal(triangle.b);
            var normal3 = mesh.CalcVertexNormal(triangle.c);

            var intAInner = AppendIfNotExists(verticesInNewMesh, vertex1 - normal1*4, newMesh);
            var intBInner = AppendIfNotExists(verticesInNewMesh, vertex3 - normal3*4, newMesh); //swapping to mirror
            var intCInner = AppendIfNotExists(verticesInNewMesh, vertex2 - normal2*4, newMesh);
            var intAInnerOldMesh = AppendIfNotExists(verticesInOldMesh, vertex1 - normal1 * 4, mesh);
            var intBInnerOldMesh = AppendIfNotExists(verticesInOldMesh, vertex2 - normal2 * 4, mesh);
            var intCInnerOldMesh = AppendIfNotExists(verticesInOldMesh, vertex3 - normal3 * 4, mesh);
            newMesh.SetVertexColor(intAInner, ColorManager.Instance.currentColor.toVector3f());
            newMesh.SetVertexColor(intBInner, ColorManager.Instance.currentColor.toVector3f());
            newMesh.SetVertexColor(intCInner, ColorManager.Instance.currentColor.toVector3f());
            mesh.SetVertexColor(intAInnerOldMesh, ColorManager.Instance.currentColor.toVector3f());
            mesh.SetVertexColor(intBInnerOldMesh, ColorManager.Instance.currentColor.toVector3f());
            mesh.SetVertexColor(intCInnerOldMesh, ColorManager.Instance.currentColor.toVector3f());
            var newTriInner = newMesh.AppendTriangle(intAInner, intBInner, intCInner);
            var newTriInnerOldMesh = mesh.AppendTriangle(intAInnerOldMesh, intBInnerOldMesh, intCInnerOldMesh);
            InnerToOuter.AddIfNotExists(intAOuter, intAInner);
            InnerToOuter.AddIfNotExists(intBOuter, intCInner);
            InnerToOuter.AddIfNotExists(intCOuter, intBInner);
            InnerToOuterOldMesh.AddIfNotExists(triangle.a, intAInnerOldMesh);
            InnerToOuterOldMesh.AddIfNotExists(triangle.b, intBInnerOldMesh);
            InnerToOuterOldMesh.AddIfNotExists(triangle.c, intCInnerOldMesh);
        }
        painted.ForEach(index => mesh.RemoveTriangle(index));

        var openEdges = newMesh.BoundaryEdgeIndices();
        foreach (var openEdge in openEdges)
        {
            var edgeOriented = newMesh.GetOrientedBoundaryEdgeV(openEdge);
            int thirdPoint = Corresponding(InnerToOuter,edgeOriented.a);
            var newTriSide = newMesh.AppendTriangle(edgeOriented.b, edgeOriented.a, thirdPoint);
        }
        var openEdgesOldMesh = mesh.BoundaryEdgeIndices();
        foreach (var openEdge in openEdgesOldMesh)
        {
            var edgeOriented = mesh.GetOrientedBoundaryEdgeV(openEdge);
            int thirdPoint = Corresponding(InnerToOuterOldMesh, edgeOriented.a);
            var newTriSide = mesh.AppendTriangle(edgeOriented.b, edgeOriented.a, thirdPoint);
        }

        var newObj = StaticFunctions.SpawnNewObject(newMesh, originalMesh);
        //newObj.transform.position += Vector3.forward;
        mesh = g3UnityUtils.SetGOMesh(gameObject, mesh);
    }

    private bool CheckPositionValid(DMesh3 mesh, Vector3d position)
    {
        if (spatial.Mesh == null)
        {
            spatial = new DMeshAABBTree3(originalMesh);
            spatial.Build();
        }
        int near_tid = spatial.FindNearestTriangle(position);
        if (near_tid != DMesh3.InvalidID)
        {
            DistPoint3Triangle3 dist = MeshQueries.TriangleDistance(mesh, near_tid, position);
            Vector3d nearest_pt = dist.TriangleClosest;
            if (dist.DistanceSquared > 3) return true;
            else return false;
        }

        return true;
    }

    private int Corresponding(Dictionary<int, int> InnerToOuter, int searchFor)
    {
        if (InnerToOuter.ContainsKey(searchFor)) return InnerToOuter[searchFor];
        if (InnerToOuter.ContainsValue(searchFor))
            return InnerToOuter.First(tuple => tuple.Value == searchFor).Key;
        else return Int32.MaxValue;
        
    }
}
