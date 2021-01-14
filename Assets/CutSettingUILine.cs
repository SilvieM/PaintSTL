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
        if (algo == Algorithm.AlgorithmType.Ignore)
        {
            ModifierDropdown.gameObject.SetActive(false);
            depthField.gameObject.SetActive(false);
        }
        else
        {
            ModifierDropdown.gameObject.SetActive(true);
            depthField.gameObject.SetActive(true);
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
