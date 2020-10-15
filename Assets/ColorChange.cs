using System.Collections;
using System.Collections.Generic;
using Assets;
using HSVPicker;
using UnityEngine;

public class ColorChange : MonoBehaviour
{
    private ColorPicker picker;
    // Start is called before the first frame update
    void Start()
    {
        picker = GetComponent<ColorPicker>();
        picker.onValueChanged.AddListener(color =>
        {
            ColorManager.Instance.currentColor=color;
        });
        ColorManager.Instance.currentColor = picker.CurrentColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
