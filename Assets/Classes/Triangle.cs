using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        public Triangle()
        {
        }

        public Triangle abNeighbor;
        public Triangle bcNeighbor;
        public Triangle caNeighbor;
        public void CalcDirectNeighbors()
        {
            a.belongsTo.ForEach(triangle =>
            {
                if (triangle != this)
                {
                    if (b.belongsTo.Contains(triangle))abNeighbor = triangle;
                    if (c.belongsTo.Contains(triangle)) caNeighbor = triangle;
                }
            });
            b.belongsTo.ForEach(triangle =>
            {
                if(triangle != this && c.belongsTo.Contains(triangle)) bcNeighbor = triangle;
            });
        }

        //public bool PointIsOnCornerOf(Vector3 point)
        //{
        //    if (point == a || point == b || point == c) return true;
        //    return false;
        //}
        //public bool IsAdjacent(Triangle other)
        //{
        //    if (PointIsOnCornerOf(other.a))
        //    {
        //        if (PointIsOnCornerOf(other.b) || PointIsOnCornerOf(other.c)) return true;
        //    }
        //    if (PointIsOnCornerOf(other.b))
        //    {
        //        if (PointIsOnCornerOf(other.a) || PointIsOnCornerOf(other.c)) return true;
        //    }
        //    if (PointIsOnCornerOf(other.c))
        //    {
        //        if (PointIsOnCornerOf(other.a) || PointIsOnCornerOf(other.b)) return true;
        //    }

        //    return false;
        //}

        //public bool TryAddAsNeighbor(Triangle potentialNeighbor)
        //{
        //    if (potentialNeighbor.PointIsOnCornerOf(a))
        //    {
        //        if (potentialNeighbor.PointIsOnCornerOf(b))
        //        {
        //            abNeighbor = potentialNeighbor;
        //            return true;
        //        }

        //        if (potentialNeighbor.PointIsOnCornerOf(c))
        //        {
        //            caNeighbor = potentialNeighbor;
        //            return true;
        //        }

        //    }
        //    if (potentialNeighbor.PointIsOnCornerOf(b) && potentialNeighbor.PointIsOnCornerOf(c))
        //    {
        //        bcNeighbor = potentialNeighbor;
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
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


    }
}