using System.Collections;
using System.Collections.Generic;
using Assets;
using UnityEngine;
using UnityEngine.UI;

public class ToolSelector : MonoBehaviour
{
    private Image brush;
    private Image bucket;
    private Image refiner;

    public enum Tools { Brush, Bucket}
    // Start is called before the first frame update
    void Start()
    {
        var symbols = GetComponentsInChildren<Image>();
        brush = symbols[0];
        bucket = symbols[1];
        refiner = symbols[2];
        brush.color = Color.red;
        ColorManager.Instance.currentTool = ColorManager.Tools.Brush;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeToolBucket()
    {
        brush.color = Color.black;
        bucket.color = Color.red;
        refiner.color = Color.black;
        ColorManager.Instance.currentTool = ColorManager.Tools.Bucket;
    }

    public void ChangeToolBrush()
    {
        brush.color = Color.red;
        bucket.color = Color.black;
        refiner.color = Color.black;
        ColorManager.Instance.currentTool = ColorManager.Tools.Brush;
    }

    public void ChangeToolRefiner()
    {
        brush.color = Color.black;
        bucket.color = Color.black;
        refiner.color = Color.red;
        ColorManager.Instance.currentTool = ColorManager.Tools.Refiner;
    }
}
