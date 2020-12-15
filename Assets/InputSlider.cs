using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputSlider : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private TMP_InputField inputField;

    private double currentValue;
    private Vector2 pos;
    // Start is called before the first frame update
    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Dragging");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        var dragAmount = eventData.position.x - pos.x;
        currentValue += dragAmount / 100;
        if (currentValue < 0) currentValue = 0;
        currentValue = Math.Round(currentValue, 2);
        inputField.text = currentValue.ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        pos = eventData.position;
    }
}
