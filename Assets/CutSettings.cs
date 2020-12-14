using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Assets.Classes;
using UnityEngine;
using UnityEngine.UI;

public class CutSettings : MonoBehaviour
{
    public List<GameObject> children;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<CutSettingData> GetSettings()
    {
        var data = new List<CutSettingData>();
        for (var index = 0; index < children.Count; index++)
        {
            var child = children[index];
            var depth = Double.Parse(child.GetComponentInChildren<InputField>().text);
            var dropdown = child.GetComponentInChildren<TMPro.TMP_Dropdown>();
            var algo = (Algorithm.AlgorithmType) dropdown.value;
            data.Add(new CutSettingData(index+1, algo, depth )); //because the base color was left out
            
        }

        return data;
    }

    public void Populate()
    {
        var childCount = transform.childCount;
        GameObject colors = (GameObject)Resources.Load("CutSettingColor");

        var list = ColorManager.Instance.GetUsedColorsWithoutBase();
        for (var index = childCount; index < list.Count; index++)
        {
            var usedColor = list[index];
            var instance = Instantiate(colors, transform);
            instance.GetComponentInChildren<Image>().color = usedColor;
            children.Add(instance);
        }
    }
}
