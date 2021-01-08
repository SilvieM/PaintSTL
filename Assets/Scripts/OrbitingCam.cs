using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitingCam : MonoBehaviour
{
    private GameObject target;

    private Vector3 originalPosition;

    private Quaternion originalRotation;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetTarget(GameObject target, Vector3 viewdistance)
    {
        this.target = target;
        transform.position = viewdistance;
        transform.LookAt(Vector3.zero);
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public void ResetCam()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            var mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            transform.RotateAround(Vector3.zero, transform.up, mouseMovement.x*5);
            transform.RotateAround(Vector3.zero, -transform.right, mouseMovement.y*5);
        }

        transform.position += GetInputTranslationDirection()*10*Time.deltaTime;
    }
    Vector3 GetInputTranslationDirection()
    {
        Vector3 direction = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            direction += transform.up;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction += -transform.up;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction += -transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += transform.right;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            direction += -transform.forward;
        }
        if (Input.GetKey(KeyCode.E))
        {
            direction += transform.forward;
        }

        if (Input.mouseScrollDelta != Vector2.zero)
        {
            direction += Input.mouseScrollDelta.y*5*transform.forward;
        }


        if (Input.GetMouseButton(2))
        {
            var mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            direction += -transform.up*mouseMovement.y + -transform.right * mouseMovement.x;
        }
        return direction;
    }
}
