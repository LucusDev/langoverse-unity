using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoadData : MonoBehaviour
{

    private string URL = "https://jsonplaceholder.typicode.com/posts";
    // Start is called before the first frame update
    void Awake()
    {


        StartCoroutine(GetDatas());
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator GetDatas()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(URL))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
                Debug.LogError(request.error);

            else
            {
                string json = request.downloadHandler.text;
                // JsonUtility.FromJson<>(json);
                Debug.Log(json);
            }
        }

    }
}
