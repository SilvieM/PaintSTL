using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Assets.Classes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class CutSettings : MonoBehaviour
{
    public List<GameObject> cutSettingColorGameObjects;

    public GameObject cutSettingsContainer;

    public GameObject minDepthField;

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
        var minDepth = Double.Parse(minDepthField.GetComponent<TMPro.TMP_InputField>().text);

        for (var index = 0; index < cutSettingColorGameObjects.Count; index++)
        {
            var child = cutSettingColorGameObjects[index];
            var depth = Double.Parse(child.GetComponentInChildren<TMPro.TMP_InputField>().text);
            var dropdowns = child.GetComponentsInChildren<TMPro.TMP_Dropdown>();
            var algo = (Algorithm.AlgorithmType) dropdowns[0].value;
            var modifier = (CutSettingData.Modifier) dropdowns[1].value;
            data.Add(new CutSettingData(index+1, algo, depth, modifier, minDepth )); //because the base color was left out we need index+1
            
        }

        return data;
    }

    public void Populate()
    {
        var childCount = cutSettingsContainer.transform.childCount;
        GameObject colors = (GameObject)Resources.Load("CutSettingColor");

        var list = ColorManager.Instance.GetUsedColorsWithoutBase();
        for (var index = childCount; index < list.Count; index++)
        {
            var usedColor = list[index];
            var instance = Instantiate(colors, cutSettingsContainer.transform);
            instance.GetComponentInChildren<Image>().color = usedColor;
            cutSettingColorGameObjects.Add(instance);
        }
    }
}
