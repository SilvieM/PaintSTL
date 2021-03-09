using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Assets;
using UnityEngine;
using UnityEngine.UI;

public class UsedColorsDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ColorManager.Instance.OnColorsChanged += OnColorsChange;
        ColorManager.Instance.OnCurrentColorChanged += OnCurrentColorChange;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnColorsChange(List<Color> colors)
    {
        var colorBox = Resources.Load<GameObject>("ColorBox");
        var existingChildren = gameObject.GetComponentsInChildren<ColorBox>();
        if (existingChildren.Length > colors.Count)
        {
            foreach (var existingChild in existingChildren)
            {
                Destroy(existingChild.gameObject);
            }
            existingChildren = new ColorBox[0];
        }
        
        for (var index = 0; index < colors.Count; index++)
        {
            var color = colors[index];
            if (existingChildren.Length > index) existingChildren[index].GetComponent<Image>().color = color;
            else
            {
                var instantiated = Instantiate(colorBox, transform);
                instantiated.GetComponent<Image>().color = color;
            }
        }
    }

    public void OnCurrentColorChange(int color)
    {
        var existingChildren = gameObject.GetComponentsInChildren<ColorBox>();
        foreach (var existingChild in existingChildren)
        {
            existingChild.UnSetBorder();
        }
        if(existingChildren.Length>= color) existingChildren[color].SetBorder();
    }
}
