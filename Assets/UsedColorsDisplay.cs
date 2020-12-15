using System.Collections.Generic;
using System.Linq;
using Assets;
using UnityEngine;
using UnityEngine.UI;

public class UsedColorsDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ColorManager.Instance.OnColorsChanged += OnColorsChange;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnColorsChange(List<Color> colors)
    {
        var colorBox = Resources.Load<GameObject>("ColorBox");
        var existingChildren = gameObject.GetComponentsInChildren<Image>().Skip(1).ToArray(); //This actually also returns the Image Component of itself as first element, so skip first
        for (var index = 0; index < colors.Count; index++)
        {
            var color = colors[index];
            if (existingChildren.Length > index) existingChildren[index].color = color;
            else
            {
                var instantiated = Instantiate(colorBox, transform);
                instantiated.GetComponent<Image>().color = color;
            }
        }
    }
}
