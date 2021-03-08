using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrushSizeSlider : MonoBehaviour
{
    private Slider mainSlider;
    private Text text;

    // Start is called before the first frame update
    void Start()
    {
        mainSlider = GetComponent<Slider>();
        mainSlider.onValueChanged.AddListener(delegate { OnSliderWasChanged(); });
        text = GetComponentInChildren<Text>();
        OnSliderWasChanged();
    }

    public void OnSliderWasChanged()
    {
        OnMeshClick[] components = GameObject.FindObjectsOfType<OnMeshClick>();
        foreach (var onMeshClick in components)
        {
            onMeshClick.range = mainSlider.value;
        }

        text.text = $"Brushsize: {mainSlider.value.ToString("F2")}mm";
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
