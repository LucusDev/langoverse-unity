using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using FlutterUnityIntegration;

public class hotel : MonoBehaviour
{
    public new Camera camera;

    void Start()
    {
       
    }

    void Update()
    {

        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {

            Ray ray = camera.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow, 100f);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {

                    GameObject touchedObject = hit.transform.gameObject;
                    UnityMessageManager.Instance.SendMessageToFlutter(touchedObject.transform.name);

                    Debug.Log("Touched " + touchedObject.transform.name);
                }
            }
        }
    }
   

}

