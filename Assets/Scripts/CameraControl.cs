using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // The object to rotate around
    public float distanceToTarget = 10.0f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 1.0f;
    public float minVerticalAngle = -80.0f;
    public float maxVerticalAngle = 80.0f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.5f;
    public float mouseWheelZoomSpeed = 1.0f; // New setting for mouse wheel sensitivity
    public float minZoomDistance = 2.0f;
    public float maxZoomDistance = 20.0f;

    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private Vector2 lastTouchPosition;
    private Vector2 lastMousePosition;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize rotation to current camera rotation
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;

        // Make sure we have a target
        if (target == null)
        {
            Debug.LogWarning("No target assigned to CameraControl! Creating an empty target at world origin.");
            GameObject targetObj = new GameObject("CameraTarget");
            target = targetObj.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Handle mouse input
        HandleMouseInput();

        // Handle touch input
        HandleTouchInput();

        // Apply rotation and position
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distanceToTarget) + target.position;

        transform.rotation = rotation;
        transform.position = position;
    }

    private void HandleMouseInput()
    {
        // Mouse rotation
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastMousePosition;
            
            // Rotate camera
            currentX += delta.x * rotationSpeed * 0.1f;
            currentY -= delta.y * rotationSpeed * 0.1f;
            
            // Clamp vertical rotation
            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
            
            lastMousePosition = Input.mousePosition;
        }

        // Mouse wheel zoom
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel != 0)
        {
            distanceToTarget -= scrollWheel * mouseWheelZoomSpeed * 10f;
            distanceToTarget = Mathf.Clamp(distanceToTarget, minZoomDistance, maxZoomDistance);
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            // Single touch for rotation
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    lastTouchPosition = touch.position;
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    Vector2 delta = touch.position - lastTouchPosition;
                    
                    // Rotate camera
                    currentX += delta.x * rotationSpeed * 0.1f;
                    currentY -= delta.y * rotationSpeed * 0.1f;
                    
                    // Clamp vertical rotation
                    currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
                    
                    lastTouchPosition = touch.position;
                }
            }
            // Two finger touch for zooming
            else if (Input.touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                // Get the previous and current positions of touches
                Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
                Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

                // Find the magnitude of the distance between touches, both previous and current
                float prevTouchDeltaMag = (touch0PrevPos - touch1PrevPos).magnitude;
                float touchDeltaMag = (touch0.position - touch1.position).magnitude;

                // Calculate the difference in magnitude between the previous and current touch distances
                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                // Apply zooming
                distanceToTarget += deltaMagnitudeDiff * zoomSpeed * 0.01f;
                distanceToTarget = Mathf.Clamp(distanceToTarget, minZoomDistance, maxZoomDistance);
            }
        }
    }
}
