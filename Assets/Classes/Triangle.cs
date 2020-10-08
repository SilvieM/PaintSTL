using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

namespace Assets.Classes
{
    public class Triangle
    {
        public Vertex a;
        public Vertex b;
        public Vertex c;
        public Vector3 n;
        public int vertexNumberOfA;
        public int subMeshNumber;
        public Color color;
        public  Edge EdgeAb;
        public  Edge EdgeBc;
        public  Edge EdgeCa;
        public Triangle(Vertex a, Vertex b, Vertex c, Vector3 n)
        {
            initTriangle(a, b, c, n);
            //var compareN = n.normalized;
            //var prelimN = Vector3.Cross(c.pos - a.pos, b.pos - a.pos).normalized;
        }

        public Triangle(Vertex a, Vertex b, Vertex c, Vector3 n, Vector3 normalOfOther)
        {
            var prelimN = Vector3.Cross(c.pos - a.pos, b.pos - a.pos).normalized;
            //the Dot product of this tri and the other colored tri should be smaller than 0 = obtuse angle (weiter Winkel) so that normal always "looks" away => outside
            if (Vector3.Dot(prelimN, normalOfOther) < 0) initTriangle(a, b, c, prelimN);  
            else initTriangle(b,a,c,-prelimN); //if it was in the wrong orientation, also turn around b and a so that they go in the same direction always
        }

        private void initTriangle(Vertex a, Vertex b, Vertex c, Vector3 n)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.a.AddBelongsTo(this);
            this.b.AddBelongsTo(this);
            this.c.AddBelongsTo(this);
            EdgeAb = new Edge(a, b, this);
            EdgeBc = new Edge(b, c, this);
            EdgeCa = new Edge(c, a, this);
            this.n = n.normalized;
        }

        public Triangle abNeighbor;
        public Triangle bcNeighbor;
        public Triangle caNeighbor;
        public void CalcDirectNeighbors()
        {
            //if (abNeighbor != null && bcNeighbor != null && caNeighbor != null) return; //If all neighbors are there, it shall not change anymore from here.
            a.belongsTo.ForEach(triangle =>
            {
                if (triangle != this)
                {
                    if (b.belongsTo.Contains(triangle))
                    {
                        //We do only overwrite the neighbor if we found a new neighbor of the same color
                        if (abNeighbor == null || this.color == triangle.color) abNeighbor = triangle;
                    }

                    if (c.belongsTo.Contains(triangle))
                    {
                        if (caNeighbor == null || this.color == triangle.color) caNeighbor = triangle;
                    }
                }
            });
            b.belongsTo.ForEach(triangle =>
            {
                if(triangle != this && c.belongsTo.Contains(triangle))
                    if (bcNeighbor == null || this.color == triangle.color)
                    {
                        bcNeighbor = triangle;
                    }
            });
        }

        public Vertex[] Vertices => new[] {a, b, c};

        public Plane TriPlane => new Plane(a.pos, n);

        public Plane PlaneAb => new Plane(EdgeAb.vertex1.pos, Vector3.Cross(n, EdgeAb.Delta));
        public Plane PlaneBc => new Plane(EdgeBc.vertex1.pos, Vector3.Cross(n, EdgeBc.Delta));
        public Plane PlaneCa => new Plane(EdgeCa.vertex1.pos, Vector3.Cross(n, EdgeCa.Delta));
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
                return (a == other.a) && (b == other.b) && (c == other.c); //Right now we never have the case of generating triangles that have their points in other orders.
            }
        }

        public Vector3 ClosestPointTo(Vector3 p)
        {
            // Find the projection of the point onto the edge

            var uab = EdgeAb.Project(p);
            var uca = EdgeCa.Project(p);

            if (uca > 1 && uab < 0)
                return a.pos;

            var ubc = EdgeBc.Project(p);

            if (uab > 1 && ubc < 0)
                return b.pos;

            if (ubc > 1 && uca < 0)
                return c.pos;

            if ((uab>=0||uab<=1) && !PlaneAb.IsAbove(p))
                return EdgeAb.PointAt((float)uab);

            if ((ubc >= 0 || ubc <= 1) && !PlaneBc.IsAbove(p))
                return EdgeBc.PointAt(ubc);

            if ((uca >= 0 || uca <= 1) && !PlaneCa.IsAbove(p))
                return EdgeCa.PointAt(uca);

            // The closest point is in the triangle so 
            // project to the plane to find it
            return TriPlane.Project(p);

        }

        public Vertex getCommonCorner(Triangle other)
        {
            if (this.a == other.a) return a;
            foreach (var vertex in this.Vertices)
            {
                if (vertex == other.a) return other.a;
                if (vertex == other.b) return other.b;
                if (vertex == other.c) return other.c;
            }

            return null;
        }


    }
}