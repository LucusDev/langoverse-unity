using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerTom : MonoBehaviour
{

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void BackFromInteriorScene()
    {
        string MapSceneName = PlayerPrefs.GetString("MapSceneName");
        SceneManager.LoadScene(MapSceneName);
        Debug.Log("latestBack");
    }

    public void BackFromMap()
    {
        SceneManager.LoadScene("mapManager");
    }
}
