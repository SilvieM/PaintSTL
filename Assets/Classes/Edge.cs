using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Classes
{
    public class Edge
    {
        public Vertex vertex1;
        public Vertex vertex2;
        public readonly Vector3 Delta;
        public Triangle belongsTo;
        public Triangle belongsTo2
        {
            get
            {

                 if (belongsTo.EdgeAb == this) return belongsTo.abNeighbor;
                 if (belongsTo.EdgeBc == this) return belongsTo.bcNeighbor;
                 if (belongsTo.EdgeCa == this) return belongsTo.caNeighbor;
                 else return null;
            }
        }

        public Edge(Vertex vertex1, Vertex vertex2, Triangle belongsTo)
        {
            this.vertex1 = vertex1;
            this.vertex2 = vertex2;
            this.belongsTo = belongsTo;
            this.Delta = this.vertex2.pos - this.vertex1.pos;
        }

        public Vector3 PointAt(float t) => vertex1.pos + t * Delta;
        public float LengthSquared => Delta.sqrMagnitude;

        public float Project(Vector3 p) => Vector3.Dot((p - vertex1.pos),Delta) / LengthSquared;
    }
}