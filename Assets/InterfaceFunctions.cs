using UnityEngine;
using UnityEngine.UI;

public class InterfaceFunctions : MonoBehaviour
{
    private GameObject cuttingUI;

    private GameObject paintingUI;

    // Start is called before the first frame update
    void Start()
    {
        cuttingUI = transform.Find("Cutting UI").gameObject;
        paintingUI = transform.Find("Painting UI").gameObject;
        cuttingUI.SetActive(false);
        paintingUI.SetActive(true);
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

    }

    public void SwitchToPaintingUI()
    {
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

        var slider = GameObject.FindObjectOfType<OpacitySlider>();
        slider.ResetToOpaque();
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
            generate.Explode();
        }
    }

    public void MovePoint()
    {
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            //generate.MovePoint();
        }
    }

    public void DisplayNormals()
    {
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            //generate.DisplayNormals();
        }
    }


    public void MakeNewPartOnePointAlgo()
    {
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            generate.Cut(Algorithm.AlgorithmType.OnePoint);
        }
    }

    public void MakeNewPartPeprAlgo()
    {
        var objects = GameObject.FindObjectsOfType<Generate>();
        foreach (var generate in objects)
        {
            generate.Cut(Algorithm.AlgorithmType.Pepr);
        }
    }

    public void ExportSTL()
    {
        //TODO open file dialog
        //TODO call export on all Generate from here
    }


}
