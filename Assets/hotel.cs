using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlutterUnityIntegration;

public class hotel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //RaycastHit hit = new RaycastHit();
        //for (int i = 0; i < Input.touchCount; ++i)
        //{
        //    if (Input.GetTouch(i).phase.Equals(TouchPhase.Began))
        //    {
        //        // Construct a ray from the current touch coordinates
        //        Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);
        //        if (Physics.Raycast(ray, out hit))
        //        {

        //        }
        //    }
        //}
        UnityMessageManager.Instance.SendMessageToFlutter("your-message-here");


    }
}
