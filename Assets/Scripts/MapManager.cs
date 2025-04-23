using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    public string CurrentMapSceneName { get; private set; } = null;
    public AssetBundle CurrentMapBundle { get; private set; } = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes if needed
        }
    }

    public void SetCurrentMap(string sceneName, AssetBundle bundle)
    {
        // Unload the current map if one is already loaded
        if (!string.IsNullOrEmpty(CurrentMapSceneName) || CurrentMapBundle != null)
        {
            UnloadCurrentMap();
        }

        CurrentMapSceneName = sceneName;
        CurrentMapBundle = bundle;
    }

public void UnloadCurrentMap()
{
    if (!string.IsNullOrEmpty(CurrentMapSceneName))
    {
        var scene = SceneManager.GetSceneByName(CurrentMapSceneName);
        if (scene.isLoaded) // Check if the scene is actually loaded
        {
            SceneManager.UnloadSceneAsync(CurrentMapSceneName);
        }
        CurrentMapSceneName = null;
    }

    if (CurrentMapBundle != null)
    {
        CurrentMapBundle.Unload(true);
        CurrentMapBundle = null;
    }
}
}

