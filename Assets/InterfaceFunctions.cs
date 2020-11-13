using UnityEngine;
using UnityEngine.UI;

public class InterfaceFunctions : MonoBehaviour
{
    Slider mainSlider;
    // Start is called before the first frame update
    void Start()
    {
        mainSlider = GetComponentInChildren<Slider>();
        mainSlider.onValueChanged.AddListener(delegate { OnSliderWasChanged(); });
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnSliderWasChanged()
    {
        //Debug.Log(mainSlider.value);
        OnMeshClick[] components = GameObject.FindObjectsOfType<OnMeshClick>();
        foreach (var onMeshClick in components)
        {
            var meshRenderers = onMeshClick.transform.GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in meshRenderers)
            {
                if(mainSlider.value>=1) meshRenderer.material = Resources.Load("STLMeshMaterial2") as Material;
                if (mainSlider.value < 1)
                {
                    meshRenderer.material = Resources.Load("SeeThruSTLMeshMaterial") as Material;
                    meshRenderer.material.SetFloat("Vector1", mainSlider.value);
                    meshRenderer.material.SetFloat("Alpha", mainSlider.value);

                }
            }
        }
    }

    public void FixMyPaintJob()
    {
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            generate.FixMyPaintJob();
        }
    }

    public void Split()
    {
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            //generate.Split();
        }
    }

    public void MovePoint()
    {
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            //generate.MovePoint();
        }
    }

    public void DisplayNormals()
    {
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            //generate.DisplayNormals();
        }
    }


    public void MakeNewPartOnePointAlgo()
    {
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            generate.MakeNewPartOnePointAlgo();
        }
    }

    public void MakeNewPartPeprAlgo()
    {
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            generate.CutPeprAlgo();
        }
    }


}
