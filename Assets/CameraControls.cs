using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraControls : MonoBehaviour
{
    private float rotationSpeed = 50.0f;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        CamOrbit();
    }
    private void CamOrbit()
    {
        if (Input.touchCount == 1)
        {
            if (Input.GetAxis("Mouse Y") != 0 || Input.GetAxis("Mouse X") != 0)
            {
                float verticalInput = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
                float horizontalInput = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
                transform.Rotate(Vector3.right, verticalInput);
                transform.Rotate(Vector3.down, horizontalInput, Space.World);
            }
        }
    }
   
}
