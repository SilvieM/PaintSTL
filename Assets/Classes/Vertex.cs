using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Classes
{
    public class Vertex
    {
        public Vector3 pos;
        public List<Triangle> belongsTo { get; private set; }
        public int number;
        public int subMeshNumber;

        public Vertex(Vector3 pos, int vertexNumber, int subMeshNumber)
        {
            this.pos = pos;
            this.belongsTo = new List<Triangle>();
            this.number = vertexNumber;
            this.subMeshNumber = subMeshNumber;
        }

        public void AddBelongsTo(Triangle toAdd)
        {
            if(!belongsTo.Contains(toAdd)) belongsTo.Add(toAdd);
        }
    }
}