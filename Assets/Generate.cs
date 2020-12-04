using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Assets;
using Assets.g3UnityUtils;
using g3;
using UnityEngine;

public class Generate : MonoBehaviour
{
    public DMesh3 mesh;
    public DMeshAABBTree3 spatial;
    private List<int> PointsToMove;
    public bool isImported;
    public DMesh3 originalMesh;
    public Vector3d center;

    public void Start()
    {
    }

    public void RefreshPointsToMove()
    {
        var color = ColorManager.Instance.currentColor.toVector3f();
        PointsToMove = mesh.VertexIndices().Where(index =>
            mesh.GetVertexColor(index) == color).ToList(); //Ressource-hungry??
    }

    public void Update()
    {
        //if (!(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow) ||
        //      Input.GetKey(KeyCode.LeftArrow))) return;
        //Debug.Log(PointsToMove.Count);
        //foreach (var PointToMove in PointsToMove)
        //{
        //    var normal = mesh.GetVertexNormal(PointToMove);
        //    var tri = mesh.GetVertex(PointToMove);
        //    var newPos= tri;
        //    if (Input.GetKey(KeyCode.UpArrow))
        //    {
        //        newPos = tri + normal * 0.1f;
        //    }
        //    if (Input.GetKey(KeyCode.DownArrow))
        //    {
        //        newPos = tri - normal * 0.1f;
        //    }
        //    if (Input.GetKey(KeyCode.RightArrow))
        //    {
        //        newPos = tri + Camera.main.transform.right.toVector3d() * 0.1;
        //    }
        //    if (Input.GetKey(KeyCode.LeftArrow))
        //    {
        //        newPos = tri - Camera.main.transform.right.toVector3d() * 0.1;
        //    }
        //    if (CheckPositionValid(mesh, newPos)) 
        //        mesh.SetVertex(PointToMove, newPos);
        //}
        //g3UnityUtils.SetGOMesh(gameObject, mesh);
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
        center = mesh.GetBounds().Center;
    }

    public void Cut(Algorithm.AlgorithmType type, int colorId, double depth)
    {
        var algorithm = Algorithm.BuildAlgo(type);
        var newMesh = algorithm.Cut(mesh, colorId, depth);
        mesh = g3UnityUtils.SetGOMesh(gameObject, newMesh);
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

    private bool isInExplodedMode;
    public void Explode()
    {
        if (!isImported)
        {
            if (!isInExplodedMode)
            {
                var mainObject = FindObjectsOfType<Generate>().First(gen => gen.isImported);
                var dir = (this.center - mainObject.center).Normalized;
                Debug.Log($"{dir.x}, {dir.y}, {dir.z}");
                isInExplodedMode = true;
                transform.position += dir.toVector3();
            }
            else
            {
                transform.position = Vector3.zero;
                isInExplodedMode = false;
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
