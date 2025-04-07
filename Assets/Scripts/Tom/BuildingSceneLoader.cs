using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class BuildingSceneLoader : MonoBehaviour
{
    public string sceneAssetBundleUrl;
    private bool isSceneLoaded = false;

    private void OnMouseDown()
    {
        if (!string.IsNullOrEmpty(sceneAssetBundleUrl))
        {
            if (!isSceneLoaded)
            {
                StartCoroutine(LoadSceneFromBundle());
            }
            else
            {
                // Unload the scene if it's already loaded
                string sceneName = gameObject.name + "_Interior";
                SceneManager.UnloadSceneAsync(sceneName);
                isSceneLoaded = false;
            }
        }
    }

    private IEnumerator LoadSceneFromBundle()
    {
        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(sceneAssetBundleUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to download scene bundle: {www.error}");
                yield break;
            }

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            if (bundle == null)
            {
                Debug.LogError("Failed to load scene AssetBundle");
                yield break;
            }

            string[] scenePaths = bundle.GetAllScenePaths();
            if (scenePaths.Length > 0)
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePaths[0]);
                yield return SceneManager.LoadSceneAsync(sceneName);
                isSceneLoaded = true;
            }

            bundle.Unload(false);
        }
    }
} 