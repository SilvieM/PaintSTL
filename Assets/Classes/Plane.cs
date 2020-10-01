using UnityEngine;
using System.Collections;

namespace Assets.Classes
{
    public class Plane
    {
        public Vector3 Point;
        public Vector3 Direction;

        public Plane(Vector3 point, Vector3 direction)
        {
            Point = point;
            Direction = direction;
        }

        public bool IsAbove(Vector3 q) => Vector3.Dot(Direction, (q - Point)) > 0; //spitzer Winkel 

        public Vector3 Project(Vector3 pointToProject)
        {
            Vector3 v = pointToProject - Point;
            Vector3 d = Vector3.Project(v, Direction.normalized);
            Vector3 projectedPoint = pointToProject - d;
            return (projectedPoint);
        }
    }
}