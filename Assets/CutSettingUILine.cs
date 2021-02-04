using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CutSettingUILine : MonoBehaviour
{
    public TMP_Dropdown AlgoDropdown;
    public TMP_Dropdown ModifierDropdown;
    public TMP_InputField depthField;
    public Toggle mainToggle;

    // Start is called before the first frame update
    void Start()
    {
        OnMainInputFieldChange();
    }

    public void OnAlgoDropdownChange()
    {
        var algo = (Algorithm.AlgorithmType)AlgoDropdown.value;
        if (algo == Algorithm.AlgorithmType.Ignore||algo == Algorithm.AlgorithmType.HoleFill) //they do not have modifiers or depths
        {
            ModifierDropdown.gameObject.SetActive(false);
            depthField.gameObject.SetActive(false);
        }
        else
        {
            ModifierDropdown.gameObject.SetActive(true);
            depthField.gameObject.SetActive(true);
        }

        if (algo == Algorithm.AlgorithmType.Backside)
        {
           if(ModifierDropdown.options.Find(data => data.text == "StraightNormals") == null)
               ModifierDropdown.options.Add(new TMP_Dropdown.OptionData("StraightNormals"));
           if (ModifierDropdown.options.Find(data => data.text == "AveragedNormals") == null)
               ModifierDropdown.options.Add(new TMP_Dropdown.OptionData("AveragedNormals"));
        }
        else
        {
            ModifierDropdown.options.RemoveAll(data => data.text == "StraightNormals");
            ModifierDropdown.options.RemoveAll(data => data.text == "AveragedNormals");
        }
    }

    public void OnMainInputFieldChange()
    {
        if (mainToggle.isOn)
        {
            AlgoDropdown.gameObject.SetActive(false);
            ModifierDropdown.gameObject.SetActive(false);
            depthField.gameObject.SetActive(false);
        }
        else
        {
            AlgoDropdown.gameObject.SetActive(true);
            ModifierDropdown.gameObject.SetActive(true);
            depthField.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
