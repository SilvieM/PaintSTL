using System.Collections;
using System.Collections.Generic;
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
                meshRenderer.material.SetFloat("Vector1", mainSlider.value);
                meshRenderer.material.SetFloat("Alpha", mainSlider.value);
            }
        }
    }

    public void Generate()
    {
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            generate.GenerateMesh();
        }
    }

    public void Split()
    {
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            generate.Split();
        }
    }


}
