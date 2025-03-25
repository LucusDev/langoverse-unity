 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToLangoMap : MonoBehaviour
{
    // [SerializeField] private SceneController sceneController;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // For mobile, use Input.touchCount > 0
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject tappedObject = hit.collider.gameObject;
                // Get object name and position
                string objectName = tappedObject.name;
                if (objectName == "ExitBtn")
                {
                    SceneManager.LoadScene("LangoTest",LoadSceneMode.Single);
                    
                }
                Debug.Log($"Tapped on : {objectName}");
            }
        }
    }
}
