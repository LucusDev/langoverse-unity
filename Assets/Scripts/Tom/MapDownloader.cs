using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class MapDownloader : MonoBehaviour
{
    private string supabaseUrl = "https://nzrlskengaeocyqxgkug.supabase.co";
    private string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im56cmxza2VuZ2Flb2N5cXhna3VnIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDI1Mzg0NTYsImV4cCI6MjA1ODExNDQ1Nn0.aPqgdGBFbwm7R0G3zzUceopLSrbZ-1fY1ikdK9miRmU";

    [Serializable]
    public class MapData
    {
        public int mapid;
        public int noofzones;
        public string sceneLink;
        public int sceneVersion;
    }

    public GameObject mapButtonPrefab;
    public GameObject mapButtonContainer;

    private List<MapData> maps = new List<MapData>();
    private List<MapData> localMaps = new List<MapData>();
    private GameObject[] mapButtons;

    private async void Start()
    {
        await FetchMapDataAsync(); // Always try online first
    }

    private void LoadLocalMapData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "maps_data.json");
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                localMaps = JsonConvert.DeserializeObject<List<MapData>>(json);
                Debug.Log($"Loaded {localMaps.Count} maps from local storage");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load local maps data: {ex.Message}");
                localMaps = new List<MapData>();
            }
        }
    }

    void InitializeMapButtons()
    {
        if (mapButtons != null)
        {
            foreach (GameObject button in mapButtons)
            {
                if (button != null)
                    Destroy(button);
            }
        }

        mapButtons = new GameObject[maps.Count];
        int i = 0;
        foreach (MapData map in maps)
        {
            GameObject mapButton = Instantiate(mapButtonPrefab, mapButtonContainer.transform);
            mapButton.transform.localPosition = new Vector3(i * 300, 0, 0);
            mapButton.transform.SetParent(mapButtonContainer.transform);
            mapButtons[i] = mapButton;

            Text buttonText = mapButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = $"Load Map ID: {map.mapid}";
            }

            i++;
            mapButton.GetComponent<Button>().onClick.AddListener(() => LoadMap(map.mapid));
        }
    }

    private async Task FetchMapDataAsync()
    {
        string endpoint = $"{supabaseUrl}/rest/v1/maps";

        using (UnityWebRequest www = UnityWebRequest.Get(endpoint))
        {
            www.SetRequestHeader("apikey", supabaseKey);
            www.SetRequestHeader("Authorization", "Bearer " + supabaseKey);
            www.SetRequestHeader("Content-Type", "application/json");

            var operation = www.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = www.downloadHandler.text;
                    List<MapData> newMaps = JsonConvert.DeserializeObject<List<MapData>>(responseText);
                    Debug.Log($"Retrieved {newMaps.Count} maps from server");

                    maps = newMaps;
                    SaveMapDataToJson(newMaps);
                    LoadLocalMapData();

                    CheckAndDownloadMaps(newMaps);
                    InitializeMapButtons();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to parse maps data: {ex.Message}");
                    TryFallbackToCachedData();
                }
            }
            else
            {
                Debug.LogError($"Error retrieving maps: {www.error}");
                TryFallbackToCachedData();
            }
        }
    }

    private void TryFallbackToCachedData()
    {
        LoadLocalMapData();
        if (localMaps.Count > 0)
        {
            Debug.Log("Using cached map data");
            maps = localMaps;
            InitializeMapButtons();
        }
        else
        {
            Debug.LogError("No cached data available.");
        }
    }

    private void CheckAndDownloadMaps(List<MapData> newMaps)
    {
        foreach (MapData onlineMap in newMaps)
        {
            if (string.IsNullOrEmpty(onlineMap.sceneLink))
                continue;

            bool needsDownload = true;
            string fileName = $"map_{onlineMap.mapid}_v{onlineMap.sceneVersion}.bundle";
            string filePath = Path.Combine(Application.persistentDataPath, "Maps", fileName);

            MapData localMap = localMaps.Find(m => m.mapid == onlineMap.mapid);

            if (localMap != null && localMap.sceneVersion == onlineMap.sceneVersion && File.Exists(filePath))
            {
                needsDownload = false;
                Debug.Log($"Map {onlineMap.mapid} is up to date (v{onlineMap.sceneVersion})");
            }
            else
            {
                Debug.Log($"Map {onlineMap.mapid} needs download or update");
            }

            if (needsDownload)
            {
                StartCoroutine(DownloadMapFile(onlineMap));
            }
        }
    }

    private void SaveMapDataToJson(List<MapData> maps)
    {
        try
        {
            string json = JsonConvert.SerializeObject(maps, Formatting.Indented);
            string filePath = Path.Combine(Application.persistentDataPath, "maps_data.json");
            File.WriteAllText(filePath, json);
            Debug.Log($"Maps data saved to: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save maps data to JSON: {ex.Message}");
        }
    }

    private IEnumerator DownloadMapFile(MapData map)
    {
        if (string.IsNullOrEmpty(map.sceneLink))
        {
            Debug.LogError($"No scene link available for map ID: {map.mapid}");
            yield break;
        }

        string mapsDirectory = Path.Combine(Application.persistentDataPath, "Maps");
        if (!Directory.Exists(mapsDirectory))
            Directory.CreateDirectory(mapsDirectory);

        string fileName = $"map_{map.mapid}_v{map.sceneVersion}.bundle";
        string filePath = Path.Combine(mapsDirectory, fileName);

        CleanupOldMapVersions(map.mapid, map.sceneVersion);

        using (UnityWebRequest www = UnityWebRequest.Get(map.sceneLink))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    File.WriteAllBytes(filePath, www.downloadHandler.data);
                    Debug.Log($"Map file for ID {map.mapid} saved to: {filePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to save map file for ID {map.mapid}: {ex.Message}");
                }
            }
            else
            {
                Debug.LogError($"Error downloading map file for ID {map.mapid}: {www.error}");
            }
        }
    }

    private void CleanupOldMapVersions(int mapId, int currentVersion)
    {
        string mapsDirectory = Path.Combine(Application.persistentDataPath, "Maps");
        if (!Directory.Exists(mapsDirectory))
            return;

        try
        {
            string[] files = Directory.GetFiles(mapsDirectory, $"map_{mapId}_v*.bundle");
            foreach (string file in files)
            {
                if (!file.Contains($"map_{mapId}_v{currentVersion}.bundle"))
                {
                    File.Delete(file);
                    Debug.Log($"Deleted old map version: {Path.GetFileName(file)}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error cleaning up old map versions: {ex.Message}");
        }
    }

    private void LoadMap(int mapId)
    {
        Debug.Log($"Loading map with ID: {mapId}");
        MapData mapToLoad = maps.Find(m => m.mapid == mapId);
        if (mapToLoad == null)
        {
            Debug.LogError($"Could not find map data for ID: {mapId}");
            return;
        }

        string fileName = $"map_{mapId}_v{mapToLoad.sceneVersion}.bundle";
        string filePath = Path.Combine(Application.persistentDataPath, "Maps", fileName);

        if (File.Exists(filePath))
        {
            StartCoroutine(LoadAssetBundleFromFile(filePath, mapToLoad));
        }
        else
        {
            Debug.LogError($"Map file not found at: {filePath}");
            StartCoroutine(DownloadMapFile(mapToLoad));
        }
    }

    private IEnumerator LoadAssetBundleFromFile(string filePath, MapData map)
    {
        // Unload any currently loaded AssetBundle to avoid conflicts
        if (MapManager.Instance.CurrentMapBundle != null)
        {
            MapManager.Instance.UnloadCurrentMap();
        }

        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(filePath);
        yield return request;

        AssetBundle bundle = request.assetBundle;
        if (bundle == null)
        {
            Debug.LogError($"Failed to load AssetBundle from path: {filePath}");
            yield break;
        }

        string sceneName = null;
        try
        {
            string[] scenePaths = bundle.GetAllScenePaths();
            if (scenePaths.Length > 0)
            {
                sceneName = Path.GetFileNameWithoutExtension(scenePaths[0]);
                PlayerPrefs.SetString("MapSceneName", sceneName);
                PlayerPrefs.SetInt("MapID", map.mapid);

                // Set the current map in MapManager
                MapManager.Instance.SetCurrentMap(sceneName, bundle);
            }
            else
            {
                string[] assetNames = bundle.GetAllAssetNames();
                if (assetNames.Length > 0)
                {
                    GameObject prefab = bundle.LoadAsset<GameObject>(assetNames[0]);
                    if (prefab != null)
                        Instantiate(prefab);

                    // Set the current map in MapManager
                    MapManager.Instance.SetCurrentMap(null, bundle);
                }
                else
                {
                    Debug.LogError("No assets found in bundle");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading AssetBundle: {ex.Message}");
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
        }
    }
}
