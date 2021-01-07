using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Assets.g3UnityUtils;
using g3;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

public class OnMeshClick : MonoBehaviour
{
    public double range = 1f;

    public double AngleStop = 30;

    private Generate generate;

    public MeshFaceSelection selection;

    // Start is called before the first frame update
    public void Start()
    {
        generate = GetComponent<Generate>();
    }


    // Update is called once per frame
    void Update()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            selection = null;
            return;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100) && (hit.transform == transform || hit.transform.parent == transform))
        {
            var paintColor = ColorManager.Instance.currentColor;
            var mesh = hit.transform.gameObject.GetComponent<MeshFilter>().sharedMesh;
            int[] meshtriangles = mesh.triangles;
            var dmesh = generate.mesh;
            var rangeSquared = Math.Pow(range, 2);
            var useBrush = ColorManager.Instance.currentTool == ColorManager.Tools.Brush;
            var triIndices = new MeshFaceSelection(dmesh);

            if(useBrush) triIndices.FloodFill(hit.triangleIndex, tid => IsInRange(dmesh, hit.triangleIndex, tid, rangeSquared), eid => CheckAngle(dmesh, eid));
            else triIndices.FloodFill(hit.triangleIndex, null, eid => CheckAngle(dmesh, eid) );

            selection = triIndices; //will make it possible to highlight

            if (Input.GetMouseButton(0))
            {
                //Actually Paint
                var colorIndex = ColorManager.Instance.FieldPainted(paintColor);

                var colorsNew = mesh.colors;

                foreach (var triIndex in triIndices)
                {
                    dmesh.SetTriangleGroup(triIndex, colorIndex);
                    for (int i = 0; i < 3; i++)
                    {
                        colorsNew[meshtriangles[triIndex * 3 + i]] = paintColor;
                    }
                }
                mesh.colors = colorsNew;

            }
        }
        else
        {
            selection = null;
        }
    }


    public void OnRenderObject()
    {
        if (selection == null) return;
        CreateLineMaterial();
        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        var dmesh = generate.mesh;
        var borders = new MeshEdgeSelection(dmesh);
        borders.SelectBoundaryTriEdges(selection);
        var bordersArray = borders.ToArray();
        GL.Begin(GL.LINES);
        GL.Color(ColorManager.Instance.currentColor);
        foreach (var i in bordersArray)
        {
            var edge = dmesh.GetEdgeV(i);
            var coord = dmesh.GetVertex(edge.a);
            var coord2 = dmesh.GetVertex(edge.b);
            GL.Vertex3((float)coord.x, (float)coord.y, (float)coord.z);
            GL.Vertex3((float)coord2.x, (float)coord2.y, (float)coord2.z);
        }
        GL.End();
        GL.PopMatrix();
    }


    static Material lineMaterial;
    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            //lineMaterial = new Material(shader);
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    private bool IsInRange(DMesh3 mesh, int triIndexOriginal, int triIndex, double rangeSquared)
    {
        var triOriginal = mesh.GetTriCentroid(triIndexOriginal);
        var tri = mesh.GetTriangle(triIndex);
        var v1 = mesh.GetVertex(tri.a);
        var v2 = mesh.GetVertex(tri.b);
        var v3 = mesh.GetVertex(tri.c);
        if (v1.DistanceSquared(triOriginal) < rangeSquared) return true;
        if (v2.DistanceSquared(triOriginal) < rangeSquared) return true;
        if (v3.DistanceSquared(triOriginal) < rangeSquared) return true;
        return false;
    }

    private bool CheckAngle(DMesh3 mesh, int eid)
    {
        if (AngleStop >= 90) return true;
        var edge = mesh.GetEdgeT(eid);
        var normal1 = mesh.GetTriNormal(edge.a);
        var normal2 = mesh.GetTriNormal(edge.b);
        var angle = Vector3d.AngleD(normal1, normal2);
        if (angle < AngleStop) return true;
        else return false;
    }


}
