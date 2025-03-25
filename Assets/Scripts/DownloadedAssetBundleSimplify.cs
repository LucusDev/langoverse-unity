using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//1Xs-_irEjhLV7CTKrFRax7xaS9ApwLsCi
public class DownloadedAssetBundleSimplify : MonoBehaviour
{
    public GameObject loaderUI;
    public Slider progressSlider;

    private AssetBundle loadedAssetBundle;
    private string bundleId;



    void Awake()
    {
        bundleId = PlayerPrefs.GetString("InteriorSceneBundleId");
    }


    void Start()
    {
        Debug.Log($"Bundle ID - {bundleId}");
        LoadBundle(bundleId);
    }


    // Helper function to get direct link from Google Drive file ID
    public static string GetGoogleDriveDirectLink(string fileId)
    {
        return $"https://drive.google.com/uc?export=download&id={fileId}";
    }

    // Start is called before the first frame update
    public void LoadBundle(string fieldId)
    {

        StartCoroutine(LoadSceneAndAssetBundle_Coroutine(fieldId));

    }

    public IEnumerator LoadSceneAndAssetBundle_Coroutine(string fieldId)
    {

        GameObject go = null;
        progressSlider.value = 0;
        float sceneProgress = 0;
        loaderUI.SetActive(true);

        string assetBundleUrl = GetGoogleDriveDirectLink(fieldId);

        if (loadedAssetBundle != null && bundleId == PlayerPrefs.GetString("InteriorSceneBundleId"))
        {
            Debug.Log("AssetBundle already loaded, skipping download.");
            go = loadedAssetBundle.LoadAsset<GameObject>(loadedAssetBundle.GetAllAssetNames()[0]);
            InstantiateGameObjectFromAssetBundle(go);
            loaderUI.SetActive(false);
            yield break;
        }
        else
        {
            if (loadedAssetBundle != null)
            {
                Debug.Log($"Loaded Bundle : {loadedAssetBundle.name}");
                loadedAssetBundle.Unload(true);
                loadedAssetBundle = null;
            }


        }

        int maxRetries = 3;
        int retryCount = 0;
        bool downloadSuccess = false;

        while (retryCount < maxRetries && !downloadSuccess)
        {
            using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(assetBundleUrl))
            {
                UnityWebRequestAsyncOperation operation = www.SendWebRequest();
                while (!operation.isDone)
                {
                    sceneProgress = operation.progress;
                    progressSlider.value = Mathf.Clamp01(sceneProgress / 0.9f); // ✅ Ensure smooth transition
                    Debug.Log($"Downloading -> progress : {progressSlider.value}");

                    yield return null;
                }
                progressSlider.value = 1f;
   
                if (www.result == UnityWebRequest.Result.Success)
                {
                    loadedAssetBundle = DownloadHandlerAssetBundle.GetContent(www);

                    if (loadedAssetBundle != null)
                    {
                        Debug.Log("Asset Bundle loaded successfully.");
                        go = loadedAssetBundle.LoadAsset<GameObject>(loadedAssetBundle.GetAllAssetNames()[0]);
                        loadedAssetBundle.Unload(false);
                        yield return new WaitForEndOfFrame();
                        downloadSuccess = true;
                    }
                    else
                    {
                        Debug.LogError("Failed to load Asset Bundle.");
                    }
                }
                else
                {
                    Debug.LogError($"Error-INFO: {www.error}");
                }
            }

            if (!downloadSuccess)
            {
                retryCount++;
                Debug.LogWarning($"Retrying download... Attempt {retryCount}/{maxRetries}");
                yield return new WaitForSeconds(2); // Wait for 2 seconds before retrying
            }
        }

        if (!downloadSuccess)
        {
            SceneManager.LoadScene("LangoTest");
            yield break;
        }

        InstantiateGameObjectFromAssetBundle(go);
        loaderUI.SetActive(false);

    }

    private void InstantiateGameObjectFromAssetBundle(GameObject go)
    {
        if (go != null)
        {
            Debug.Log($"Instantiating GameObject: {go.name}");
            GameObject instanceGO = Instantiate(go);
            instanceGO.transform.position = Vector3.zero;
            // AdjustGameObjectToFitCameraView(instanceGO);

        }
        else
        {
            Debug.LogError("Failed to instantiate GameObject from Asset Bundle.");
        }
    }




}
