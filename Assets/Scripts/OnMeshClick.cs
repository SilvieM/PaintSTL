using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using UnityEngine;
using UnityEngine.Rendering;

public class OnMeshClick : MonoBehaviour
{
    //TODO make the colors be kept in sync with what the generate script builds!
    private List<Collider> childrenColliders;
    // Start is called before the first frame update
    public void Start()
    {
        var thismesh = gameObject.GetComponent<MeshFilter>();
        if (thismesh != null) thismesh.sharedMesh.colors = Enumerable.Repeat(Color.white, thismesh.sharedMesh.vertices.Length).ToArray();
        childrenColliders = new List<Collider>();
        foreach (Transform childTransform in transform)
        {
            childrenColliders.Add(childTransform.GetComponent<Collider>());
            var mesh = childTransform.gameObject.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] vertices = mesh.vertices;
            Color[] colors = Enumerable.Repeat(Color.white, vertices.Length).ToArray();
            //Color32[] colors32 = Enumerable.Repeat(new Color32(), vertices.Length).ToArray();
            mesh.colors = colors;
            //mesh.colors32 = colors32;
        }

    }


    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100) && (hit.transform == transform || hit.transform.parent == transform))
        {
            var paintColor = ColorManager.Instance.currentColor;
            var mesh = hit.transform.gameObject.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] meshtriangles = mesh.triangles;
            Vector3 p0 = vertices[meshtriangles[hit.triangleIndex * 3 + 0]];
            Vector3 p1 = vertices[meshtriangles[hit.triangleIndex * 3 + 1]];
            Vector3 p2 = vertices[meshtriangles[hit.triangleIndex * 3 + 2]];
            p0 = hit.transform.TransformPoint(p0);
            p1 = hit.transform.TransformPoint(p1);
            p2 = hit.transform.TransformPoint(p2);
            Debug.DrawLine(p0, p1, ColorManager.Instance.currentColor);
            Debug.DrawLine(p1, p2, ColorManager.Instance.currentColor);
            Debug.DrawLine(p2, p0, ColorManager.Instance.currentColor);

            if (Input.GetMouseButton(0))
            {
                var colorsNew = mesh.colors;
                for (int i = 0; i < 3; i++)
                {
                    colorsNew[hit.triangleIndex * 3 + i] = paintColor;
                }
                mesh.colors = colorsNew;

                var triangles = GetComponent<Generate>().allTriangles;

                var submeshIndex = int.Parse(mesh.name);
                var paintedTri = triangles.Where(tri =>
                    tri.subMeshNumber == submeshIndex
                && tri.vertexNumberOfA == hit.triangleIndex * 3);
                paintedTri.First().color = paintColor;
                ColorManager.Instance.FieldPainted(paintColor);
            }
        }
    }

}
