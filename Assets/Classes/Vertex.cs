using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Classes
{
    public class Vertex
    {
        public Vector3 pos;
        public List<Triangle> belongsTo;
        public int number;
        public int subMeshNumber;

        public Vertex(Vector3 pos, Triangle from, int vertexNumber, int subMeshNumber)
        {
            this.pos = pos;
            this.belongsTo = new List<Triangle>(){from};
            this.number = vertexNumber;
            this.subMeshNumber = subMeshNumber;
        }
    }
}