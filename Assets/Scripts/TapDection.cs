using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TapDection : MonoBehaviour
{
    [SerializeField] private SceneController sceneController;

    // void Awake()
    // {
    //     sceneController = FindObjectOfType<SceneController>();
    // }

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
                Vector3 objectPosition = tappedObject.transform.position;
                if (objectName != null)
                {
                    saveData(objectName);
                    // sceneController.LoadInteriorScene();
                    SceneManager.LoadScene("InteriorScene");

                }

                Debug.Log($"Tapped on : {objectName}");
                Debug.Log($"Position : {objectPosition}");
            }
        }
    }

    void saveData(string objectName)
    {
        switch (objectName)
        {
            case "Airplane":
                PlayerPrefs.SetString("InteriorSceneBundleId", "1Xs-_irEjhLV7CTKrFRax7xaS9ApwLsCi");
                break;
            case "Cube.010":
                PlayerPrefs.SetString("InteriorSceneBundleId", "1HOd7VQhR619ws-kWdCM2mFX6KrM1LTOj");
                break;
            default:
                break;
        }

    }

}
