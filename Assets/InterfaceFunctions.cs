using UnityEngine;
using UnityEngine.UI;

public class InterfaceFunctions : MonoBehaviour
{
    Slider mainSlider;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

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


}
