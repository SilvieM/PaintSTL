using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
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
            foreach (var (subMesh, subMeshIndex) in subMeshes.LoopIndex())
            {
                //var currentTriangle = new Triangle();
                //var counter = 0;
                //foreach (var (color, index) in subMesh.colors.LoopIndex())
                //{
                //    //if (color == Color.green)
                //    {
                //        var vertex = subMesh.vertices[index];
                //        switch (counter)
                //        {
                //            case 0: currentTriangle.a = vertex;
                //                currentTriangle.n = subMesh.normals[index];
                //                currentTriangle.subMeshNumber = subMeshIndex;
                //                currentTriangle.vertexNumberOfA = index;

                //                counter++;
                //                break;
                //            case 1: currentTriangle.b = vertex;
                //                counter++;
                //                break;
                //            case 2: currentTriangle.c = vertex;
                //                if(color==Color.green)
                //                    greenTriangles.Add(currentTriangle);
                //                else otherTriangles.Add(currentTriangle);
                //                currentTriangle = new Triangle();
                //                counter = 0;
                //                break;
                //        }

                //    }
                //}
                int[] t = subMesh.triangles;
                int triangleCount = t.Length;
                Vector3[] v = subMesh.vertices;
                Vector3[] n = subMesh.normals;
                for (int i = 0; i < triangleCount; i += 3)
                {
                    int a = t[i], b = t[i + 1], c = t[i + 2];
                    var triangle = new Triangle()
                    {
                        a = v[a],
                        b = v[b],
                        c = v[c],
                        n = n[a],
                        subMeshNumber = subMeshIndex,
                        vertexNumberOfA = a
                    };
                    if (subMesh.colors[i] == Color.green) greenTriangles.Add(triangle);
                    else otherTriangles.Add(triangle);

                }
            }

            Debug.Log($"Found green Triangles: {greenTriangles.Count} and other Triangles: {otherTriangles.Count} in mesh {mesh.gameObject.name}");

            foreach (var firstTriangle in greenTriangles) //if it has already found all their neighbors there cant be any more
            {
                if(firstTriangle.hasAllNeighbors) continue;
                foreach (var secondTriangle in greenTriangles)
                {
                    if(secondTriangle.hasAllNeighbors) continue; 
                    if(secondTriangle.Equals(firstTriangle)) continue;
                    if(firstTriangle.TryAddAsNeighbor(secondTriangle)) //If we could add it to one, we can also add it to the other. If not, we can save the effort.
                        secondTriangle.TryAddAsNeighbor(firstTriangle);
                }
            }

            var edges = new List<Edge>();
            foreach (var greenTriangle in greenTriangles)
            {
                if (greenTriangle.abNeighbor == null)
                {
                    edges.Add(new Edge()
                    {
                        belongsToTriangle = greenTriangle,
                        side1 = greenTriangle.a,
                        side2 = greenTriangle.b
                    });
                }
                if (greenTriangle.bcNeighbor == null)
                {
                    edges.Add(new Edge()
                    {
                        belongsToTriangle = greenTriangle,
                        side1 = greenTriangle.b,
                        side2 = greenTriangle.c
                    });
                }
                if (greenTriangle.caNeighbor == null)
                {
                    edges.Add(new Edge()
                    {
                        belongsToTriangle = greenTriangle,
                        side1 = greenTriangle.c,
                        side2 = greenTriangle.a
                    });
                }
            }

            Debug.Log($"Edges: {edges.Count}");
            foreach (var edge in edges)
            {
                //Debug.DrawLine(edge.side1/20+new Vector3(2.896f,1.54f,-0.279f), edge.side2/20 + new Vector3(2.896f, 1.54f, -0.279f), Color.red, 5, false);
                Debug.DrawLine(mesh.transform.TransformPoint(edge.side1), mesh.transform.TransformPoint(edge.side2), Color.red, 5, false);
            }

        }
    }

    public class Triangle
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
        public Vector3 n;
        public int vertexNumberOfA;
        public int subMeshNumber;
        public Triangle abNeighbor;
        public Triangle bcNeighbor;
        public Triangle caNeighbor;

        public Triangle()
        {
        }

        public bool PointIsOnCornerOf(Vector3 point)
        {
            if (point == a || point == b || point == c) return true;
            return false;
        }
        public bool IsAdjacent(Triangle other)
        {
            if (PointIsOnCornerOf(other.a))
            {
                if (PointIsOnCornerOf(other.b) || PointIsOnCornerOf(other.c)) return true;
            }
            if (PointIsOnCornerOf(other.b))
            {
                if (PointIsOnCornerOf(other.a) || PointIsOnCornerOf(other.c)) return true;
            }
            if (PointIsOnCornerOf(other.c))
            {
                if (PointIsOnCornerOf(other.a) || PointIsOnCornerOf(other.b)) return true;
            }

            return false;
        }

        public bool hasAllNeighbors => abNeighbor != null && bcNeighbor != null && caNeighbor != null;

        public bool TryAddAsNeighbor(Triangle potentialNeighbor)
        {
            if (potentialNeighbor.PointIsOnCornerOf(a))
            {
                if (potentialNeighbor.PointIsOnCornerOf(b))
                {
                    abNeighbor = potentialNeighbor;
                    return true;
                }

                if (potentialNeighbor.PointIsOnCornerOf(c))
                {
                    caNeighbor = potentialNeighbor;
                    return true;
                }
                
            }
            if (potentialNeighbor.PointIsOnCornerOf(b) && potentialNeighbor.PointIsOnCornerOf(c))
            {
                bcNeighbor = potentialNeighbor;
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool Equals(System.Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Triangle other = (Triangle)obj;
                return (a == other.a) && (b == other.b)&&(c==other.c); //Right now we never have the case of generating triangles that have their points in other orders.
            }
        }


    }

    public class Edge
    {
        public Triangle belongsToTriangle;
        public Vector3 side1;
        public Vector3 side2;
    }
}
