using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class PermanentAssetBundleManager : MonoBehaviour
{
    private static PermanentAssetBundleManager _instance;
    public static PermanentAssetBundleManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("PermanentAssetBundleManager instance is not available.");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // ✅ Prevent destruction between scenes
        }
        else
        {
            Destroy(gameObject); // ✅ Prevent duplicate instances
        }
    }

    private Dictionary<string, AssetBundle> memoryCache = new Dictionary<string, AssetBundle>();
    private HashSet<string> activeDownloads = new HashSet<string>();

    public IEnumerator DownloadAndCacheBundle(string bundleName, string url)
    {
        if (memoryCache.ContainsKey(bundleName))
        {
            Debug.Log($"Bundle {bundleName} already in memory cache.");
            yield break;
        }

        if (activeDownloads.Contains(bundleName))
        {
            Debug.Log($"Bundle {bundleName} is already being downloaded.");
            yield break;
        }

        activeDownloads.Add(bundleName);

        string cachePath = GetCachePath(bundleName);

        // 1. Try built-in cache first
        if (Caching.IsVersionCached(new CachedAssetBundle(bundleName, Hash128.Compute(bundleName))))
        {
            Debug.Log("Load from Build-In-Cache");
            yield return LoadFromUnityCache(url, bundleName);
        }
        // 2. Try manual cache
        else if (File.Exists(cachePath))
        {
            Debug.Log("Load from File-Cache");
            LoadFromDiskCache(cachePath, bundleName);
        }
        // 3. Download and cache
        else
        {
            Debug.Log("DownLoad from Server");
            yield return DownloadAndSaveBundle(url, cachePath, bundleName);
        }

        activeDownloads.Remove(bundleName);
    }

    private IEnumerator LoadFromUnityCache(string url, string bundleName)
    {
        Debug.Log(" - Sending Server Request - ");
        using (var request = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                memoryCache[bundleName] = DownloadHandlerAssetBundle.GetContent(request);
            }
        }
    }

    private void LoadFromDiskCache(string path, string bundleName)
    {
        memoryCache[bundleName] = AssetBundle.LoadFromFile(path);
    }

    private IEnumerator DownloadAndSaveBundle(string url, string savePath, string bundleName)
    {
        // Use raw download instead of AssetBundle handler
        using (var request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Save raw bytes to persistent storage
                File.WriteAllBytes(savePath, request.downloadHandler.data);

                // Load from saved file
                memoryCache[bundleName] = AssetBundle.LoadFromFile(savePath);
            }
        }
    }

    private string GetCachePath(string bundleName)
    {
        return Path.Combine(Application.persistentDataPath, $"{bundleName}.unity3d");
    }

    void OnApplicationQuit()
    {
        ClearCache();
    }

    private void ClearCache()
    {
        foreach (var bundleName in memoryCache.Keys)
        {
            string cachePath = GetCachePath(bundleName);
            if (File.Exists(cachePath))
            {
                File.Delete(cachePath);
                Debug.Log($"Deleted cache file: {cachePath}");
            }
        }
        memoryCache.Clear();
    }

    public AssetBundle GetBundle(string bundleName)
    {
        return memoryCache.TryGetValue(bundleName, out var bundle) ? bundle : null;
    }
}
