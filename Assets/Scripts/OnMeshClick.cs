using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Assets.g3UnityUtils;
using g3;
using UnityEngine;
using UnityEngine.Rendering;

public class OnMeshClick : MonoBehaviour
{
    //TODO make the colors be kept in sync with what the generate script builds!

    // Start is called before the first frame update
    public void Start()
    {
        //var thismesh = gameObject.GetComponent<MeshFilter>();
        //if (thismesh != null) thismesh.sharedMesh.colors = Enumerable.Repeat(Color.white, thismesh.sharedMesh.vertices.Length).ToArray();
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
                var generate = GetComponent<Generate>();
                var colors = generate.colorsPerTri;
                var colorsNew = mesh.colors;
                for (int i = 0; i < 3; i++)
                {
                    colorsNew[meshtriangles[hit.triangleIndex * 3 + i]] = paintColor;
                }
                mesh.colors = colorsNew;
                //Debug.Log($"Painted {hit.triangleIndex}");
                
                colors[hit.triangleIndex] = paintColor;
                var colorIndex = ColorManager.Instance.FieldPainted(paintColor);
                var dmesh = generate.mesh;
                dmesh.SetTriangleGroup(hit.triangleIndex, colorIndex);
                
                
            }
        }
    }

}
