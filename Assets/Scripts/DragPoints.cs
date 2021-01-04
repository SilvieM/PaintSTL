using System.Collections;
using Assets.g3UnityUtils;
using g3;
using UnityEngine;

namespace Assets.Scripts
{
    public class DragPoints : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
                if (GetComponent<Generate>().isImported) return;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100) &&
                    (hit.transform == transform || hit.transform.parent == transform))
                {
                    var generate = GetComponent<Generate>();
                    var triangle = generate.mesh.GetTriangle(hit.triangleIndex);
                    var coord = transform.InverseTransformPoint(hit.point);
                    var vert = GetNearestVertex(coord, generate.mesh, triangle);
                    var dir = Camera.main.transform.up;
                    var newPos = generate.mesh.GetVertex(vert) + dir.toVector3d() * 0.1f;
                    generate.mesh.SetVertex(vert, newPos);
                    generate.Redraw();
                }
            }
        }

        private int GetNearestVertex(Vector3 coord, DMesh3 mesh, Index3i triangle)
        {
            var vert1 = mesh.GetVertex(triangle.a);
            var vert2 = mesh.GetVertex(triangle.b);
            var vert3 = mesh.GetVertex(triangle.c);
            var dist1 = vert1.DistanceSquared(coord.toVector3d());
            var dist2 = vert2.DistanceSquared(coord.toVector3d());
            var dist3 = vert3.DistanceSquared(coord.toVector3d());

            if (dist1 < dist2)
            {
                if (dist1 < dist3) return triangle.a;
            }
            else
            {
                if (dist2 < dist3) return triangle.b;
                return triangle.c;
            }

            return triangle.c;
        }
    }
}