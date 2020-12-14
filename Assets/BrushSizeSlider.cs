using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrushSizeSlider : MonoBehaviour
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
            onMeshClick.range = mainSlider.value;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
