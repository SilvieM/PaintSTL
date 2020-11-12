using HSVPicker;
using UnityEngine;
using UnityEngine.UI;

public class ColorBox : MonoBehaviour
{
    private Image img;

    private ColorPicker picker;
    // Start is called before the first frame update
    void Start()
    {
        img = GetComponent<Image>();
        picker = GetComponentInParent<ColorPicker>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ColorBoxClicked()
    {
        var color = img.color;
        picker.CurrentColor = color;
    }
}
