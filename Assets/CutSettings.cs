using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public void CutSettingDropdownChanged()
    {
        
    }

    public List<CutSettingData> GetSettings()
    {
        var data = new List<CutSettingData>();
        var minDepth = Double.Parse(minDepthField.GetComponent<TMPro.TMP_InputField>().text);

        for (var index = 0; index < cutSettingColorGameObjects.Count; index++)
        {
            var child = cutSettingColorGameObjects[index];
            var line = child.GetComponent<CutSettingUILine>();
            
            var algo = (Algorithm.AlgorithmType) line.AlgoDropdown.value;
            if (algo == Algorithm.AlgorithmType.Ignore) continue;
            if (line.mainToggle.isOn)
            {
                ColorManager.Instance.MainColorId = index;
                continue; //Maincolor will not be cut, so no need to add it to data
            }
            var depth = Double.Parse(line.depthField.text);
            var modifier = (CutSettingData.Modifier) line.ModifierDropdown.value;
            var cutSettingData = new CutSettingData(index, algo, depth, modifier, minDepth );
            data.Add(cutSettingData);
        }
        return data;
    }

    public void Populate()
    {
        var toggleGroup = GetComponent<ToggleGroup>();
        
        var childCount = cutSettingsContainer.transform.childCount;
        GameObject colors = (GameObject)Resources.Load("CutSettingColor");

        var list = ColorManager.Instance.GetUsedColors().Keys.ToList();
        if (childCount > list.Count) //in that case a new obj was loaded with less colors
        {
            cutSettingColorGameObjects.ForEach(obj => Destroy(obj));
            cutSettingColorGameObjects.Clear();
            childCount = 0;
        }
        for (var index = 0; index < list.Count; index++)
        {
            if (index >= childCount) //make new
            {
                var usedColor = list[index];
                var instance = Instantiate(colors, cutSettingsContainer.transform);
                toggleGroup.RegisterToggle(instance.GetComponentInChildren<Toggle>());
                instance.GetComponentInChildren<Toggle>().group = toggleGroup;
                instance.GetComponentInChildren<Image>().color = usedColor;
                cutSettingColorGameObjects.Add(instance);
            }
            else
            {
                var usedColor = list[index];
                cutSettingsContainer.transform.GetChild(index).GetComponentInChildren<Image>().color = usedColor;
            }
        }
        
    }
}
