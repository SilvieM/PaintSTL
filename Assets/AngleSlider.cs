using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AngleSlider : MonoBehaviour
{
    private Slider mainSlider;
    private Text text;

    void Start()
    {
        mainSlider = GetComponent<Slider>();
        mainSlider.onValueChanged.AddListener(delegate { OnSliderWasChanged(); });
        text = GetComponentInChildren<Text>();
        OnSliderWasChanged();
    }
    private void OnSliderWasChanged()
    {
        OnMeshClick[] components = GameObject.FindObjectsOfType<OnMeshClick>();
        foreach (var onMeshClick in components)
        {
            onMeshClick.AngleStop = mainSlider.value;
        }
        text.text = $"Stop at angle: {mainSlider.value}°";
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
