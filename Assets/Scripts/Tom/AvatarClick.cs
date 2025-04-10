using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarClick : MonoBehaviour
{
    
    public ShareController shareController;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Check if left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            // Create a ray from the camera through the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits this object's collider
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Debug.Log("Avatar clicked!");
                    // Add your click handling code here
                    shareController.ShowConversation();
                }
            }
        }
    }
}
