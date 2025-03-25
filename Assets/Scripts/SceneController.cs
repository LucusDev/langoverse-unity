using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public GameObject loaderUI;
    public Slider progressSlider; // URL to your asset bundle on Google Drive
    // private AssetBundle loadedAssetBundle;


    void Start()
    {
          Debug.Log(" - Start Scene Controller - ");
    }



    // public void LoadInteriorScene()
    // {
    //     StartCoroutine(LoadSceneAndAssetBundle_Coroutine());
    // }

    // public IEnumerator LoadSceneAndAssetBundle_Coroutine()
    // {
    //     GameObject go = null;
    //     progressSlider.value = 0;
    //     loaderUI.SetActive(true);

    //     // Load scene after asset bundle is loaded
    //     AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("InteriorScene");
    //     asyncOperation.allowSceneActivation = false;
    //     float sceneProgress = 0;
    //     while (!asyncOperation.isDone)
    //     {
    //         sceneProgress = Mathf.MoveTowards(sceneProgress, asyncOperation.progress, Time.deltaTime);
    //         progressSlider.value = Mathf.Lerp(0.5f, 1f, sceneProgress); // Adjust progress to show scene loading after asset bundle
    //         if (sceneProgress >= 0.9f)
    //         {
    //             progressSlider.value = 1;
    //             asyncOperation.allowSceneActivation = true;
    //         }
    //         yield return null;
    //     }
    //     loaderUI.SetActive(false);
    // }


    public void pop()
    {
   
        SceneManager.LoadScene("LangoTest");
        // StartCoroutine(backToMain());
    }

    public IEnumerator backToMain()
    {
      
        progressSlider.value = 0;
        loaderUI.SetActive(true);
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("LangoTest");
        asyncOperation.allowSceneActivation = false;
        float progress = 0;
        while (!asyncOperation.isDone)
        {
            progress = Mathf.MoveTowards(progress, asyncOperation.progress, Time.deltaTime);
            progressSlider.value = progress;
            if (progress >= 0.9f)
            {
                progressSlider.value = 1;
                asyncOperation.allowSceneActivation = true;

            }
            yield return null;

        }
    }
}
