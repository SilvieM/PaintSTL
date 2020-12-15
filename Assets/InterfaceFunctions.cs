using System.Collections;
using Assets;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceFunctions : MonoBehaviour
{
    private GameObject cuttingUI;

    private GameObject paintingUI;
    private GameObject cutSettings;
    private static GameObject ErrorMsgCanvas;

    // Start is called before the first frame update
    void Start()
    {
        cuttingUI = transform.Find("Cutting UI").gameObject;
        paintingUI = transform.Find("Painting UI").gameObject;
        cuttingUI.SetActive(false);
        paintingUI.SetActive(true);
        cutSettings = cuttingUI.transform.Find("CutSettings").gameObject;
        ErrorMsgCanvas = transform.Find("MsgCanvas").gameObject;
        ErrorMsgCanvas.SetActive(false);
    }

    

    // Update is called once per frame
    void Update()
    {

    }

    public void SwitchToCuttingUI()
    {
        paintingUI.SetActive(false);
        cuttingUI.SetActive(true);
        var paintables = GameObject.FindObjectsOfType<OnMeshClick>();
        foreach (var onMeshClick in paintables)
        {
            onMeshClick.enabled = false;
        }
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            generate.SaveColored();
        }
        cutSettings.GetComponent<CutSettings>().Populate();

    }

    public void SwitchToPaintingUI()
    {
        var slider = GameObject.FindObjectOfType<OpacitySlider>();
        slider.ResetToOpaque();
        cuttingUI.SetActive(false);
        paintingUI.SetActive(true);
        var paintables = GameObject.FindObjectsOfType<OnMeshClick>();
        foreach (var onMeshClick in paintables)
        {
            onMeshClick.enabled=true;
        }
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            generate.Revert();
        }


    }

    public void FixMyPaintJob()
    {
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            generate.FixMyPaintJob();
        }
    }

    public void ExplodedView()
    {
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            if(!generate.isImported)
                generate.Explode();
        }
    }


    public void Cut()
    {
        var objects = GameObject.FindObjectsOfType<Generate>();
        var settings = cutSettings.GetComponent<CutSettings>().GetSettings();
        foreach (var cutSettingData in settings)
        {
            foreach (var generate in objects)
            {
                generate.Cut(cutSettingData);
                Debug.Log($"Cutting {cutSettingData.ColorNum} with Algo {cutSettingData.algo}");
            }
        }
    }


    public void ErrorMessage(string message)
    {
        ErrorMsgCanvas.GetComponentInChildren<Text>().text = message;
        ErrorMsgCanvas.SetActive(true);
        StartCoroutine(waitAndDeactivate());
    }

    private IEnumerator waitAndDeactivate()
    {
        yield return new WaitForSeconds(5);
        ErrorMsgCanvas.SetActive(false);
    }

    
}
