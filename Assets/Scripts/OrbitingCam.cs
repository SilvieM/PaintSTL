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
            var rotationMiddle = Vector3.zero;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                rotationMiddle = hit.point;
            }
            else
            {
                Ray ray2 = Camera.main.ScreenPointToRay(new Vector2(Screen.width/2, Screen.height/2));
                RaycastHit hit2;
                if (Physics.SphereCast(ray2, 10.0f, out hit2, 100f))
                {
                    rotationMiddle = hit2.point;
                }
            }
            var mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            transform.RotateAround(rotationMiddle, transform.up, mouseMovement.x*5);
            transform.RotateAround(rotationMiddle, -transform.right, mouseMovement.y*5);
        }

        transform.position += GetInputTranslationDirection()*0.1f;
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
