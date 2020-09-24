using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Classes
{
    public class Edge
    {
        public Vertex vertex1;
        public Vertex vertex2;
        public List<Triangle> belongsTo;

        public Edge(Vertex vertex1, Vertex vertex2, List<Triangle> belongsTo)
        {
            this.vertex1 = vertex1;
            this.vertex2 = vertex2;
            this.belongsTo = belongsTo;
        }
    }
}