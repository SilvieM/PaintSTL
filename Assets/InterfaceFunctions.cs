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
            onMeshClick.enabled = true;
        }
        ResetCutted();
    }

    public void ResetCutted()
    {
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

    public void ExplodedView(float value)
    {
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            if(!generate.isImported)
                generate.Explode(value);
        }
    }


    public void Cut()
    {
        ResetCutted();
        var generate = GameObject.FindObjectOfType<Generate>();
        var settings = cutSettings.GetComponent<CutSettings>().GetSettings();
        generate.Cut(settings);
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

    public void ResetCamera()
    {
        Camera.main.GetComponent<OrbitingCam>().ResetCam();
    }

    public void Exit()
    {
        Application.Quit();
    }
    
    
}
