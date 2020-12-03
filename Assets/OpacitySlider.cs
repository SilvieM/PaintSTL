using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpacitySlider : MonoBehaviour
{
    private Slider mainSlider;

    // Start is called before the first frame update
    void Start()
    {
        mainSlider = GetComponent<Slider>();
        mainSlider.onValueChanged.AddListener(delegate { OnSliderWasChanged(); });
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
                if (mainSlider.value >= 1) meshRenderer.material = Resources.Load("STLMeshMaterial2") as Material;
                if (mainSlider.value < 1)
                {
                    meshRenderer.material = Resources.Load("SeeThruSTLMeshMaterial") as Material;
                    meshRenderer.material.SetFloat("Vector1", mainSlider.value);
                    meshRenderer.material.SetFloat("Alpha", mainSlider.value);

                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetToOpaque()
    {
        mainSlider.value = 1;
    }
}
