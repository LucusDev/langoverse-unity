using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraControls : MonoBehaviour
{
    public float rotationSpeed = 1.0f;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CamOrbit();
    }

    //private void CamOrbit()
    //{
    //    if (Input.touchCount == 1)
    //    {
    //        if (Input.GetAxis("Mouse Y") != 0 || Input.GetAxis("Mouse X") != 0)
    //        {
    //            float verticalInput = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
    //            float horizontalInput = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
    //            transform.Rotate(Vector3.right, verticalInput);
    //            transform.Rotate(Vector3.down, horizontalInput, Space.World);
    //        }
    //    }
    //}
    private void CamOrbit()
    {
        if (Input.touchCount == 1 &&
          Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            var touchDeltaPosition = Input.GetTouch(0).deltaPosition;
            float verticalInput = touchDeltaPosition.y * rotationSpeed * Time.deltaTime;
            float horizontalInput = touchDeltaPosition.x * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.down, horizontalInput, Space.World);

        }

    }
}
