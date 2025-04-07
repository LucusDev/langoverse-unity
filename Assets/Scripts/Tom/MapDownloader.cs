using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    
    void Start()
    {
        LoadLocalMapData();
        StartCoroutine(FetchMapData());
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
        // Clear existing buttons if any
        if (mapButtons != null)
        {
            foreach (GameObject button in mapButtons)
            {
                if (button != null)
                    Destroy(button);
            }
        }
        
        // Initialize the mapButtons array with the correct size
        mapButtons = new GameObject[maps.Count];
        
        int i = 0;
        foreach (MapData map in maps)
        {
            GameObject mapButton = Instantiate(mapButtonPrefab, mapButtonContainer.transform);
            mapButton.transform.localPosition = new Vector3(i * 300, 0, 0);
            mapButton.transform.SetParent(mapButtonContainer.transform);
            mapButtons[i] = mapButton;

            // Set the button text to the map ID
            Text buttonText = mapButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = $"Load Map ID: {map.mapid}"; // Change this to the desired text format
            }

            i++;
            mapButton.GetComponent<Button>().onClick.AddListener(() => LoadMap(map.mapid));
        }
    }

    public IEnumerator FetchMapData()
    {
        string endpoint = $"{supabaseUrl}/rest/v1/maps";

        using (UnityWebRequest www = UnityWebRequest.Get(endpoint))
        {
            www.SetRequestHeader("apikey", supabaseKey);
            www.SetRequestHeader("Authorization", "Bearer " + supabaseKey);
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = www.downloadHandler.text;
                    List<MapData> newMaps = JsonConvert.DeserializeObject<List<MapData>>(responseText);
                    Debug.Log($"Retrieved {newMaps.Count} maps from server");
                    
                    this.maps = newMaps;
                    SaveMapDataToJson(newMaps);
                    
                    // Check for new maps or updated versions
                    CheckAndDownloadMaps(newMaps);

                    InitializeMapButtons();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to parse maps data: {ex.Message}");
                }
            }
            else
            {
                Debug.LogError($"Error retrieving maps: {www.error}");
                // If we have local maps data, use that instead
                if (localMaps.Count > 0)
                {
                    Debug.Log("Using cached map data");
                    maps = localMaps;
                    InitializeMapButtons();
                }
            }
        }
    }

    private void CheckAndDownloadMaps(List<MapData> newMaps)
    {
        foreach (MapData onlineMap in newMaps)
        {
            if (string.IsNullOrEmpty(onlineMap.sceneLink))
            {
                continue;
            }

            bool needsDownload = true;
            
            // Check if we have this map locally
            MapData localMap = localMaps.Find(m => m.mapid == onlineMap.mapid);
            
            // If we have the map locally, check if versions match
            if (localMap != null)
            {
                // Check if the local version is the same as server version
                if (localMap.sceneVersion == onlineMap.sceneVersion)
                {
                    // Check if the file actually exists
                    string fileName = $"map_{onlineMap.mapid}_v{onlineMap.sceneVersion}.bundle";
                    string filePath = Path.Combine(Application.persistentDataPath, "Maps", fileName);
                    
                    if (File.Exists(filePath))
                    {
                        needsDownload = false;
                        Debug.Log($"Map {onlineMap.mapid} is up to date (v{onlineMap.sceneVersion})");
                    }
                    else
                    {
                        Debug.Log($"Map {onlineMap.mapid} file missing locally, will download");
                    }
                }
                else
                {
                    Debug.Log($"Map {onlineMap.mapid} has a new version (local: v{localMap.sceneVersion}, server: v{onlineMap.sceneVersion})");
                }
            }
            else
            {
                Debug.Log($"New map found: {onlineMap.mapid} (v{onlineMap.sceneVersion})");
            }
            
            // Download if needed
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

        Debug.Log($"Downloading map file from: {map.sceneLink}");
        
        // Create directory if it doesn't exist
        string mapsDirectory = Path.Combine(Application.persistentDataPath, "Maps");
        if (!Directory.Exists(mapsDirectory))
        {
            Directory.CreateDirectory(mapsDirectory);
        }

        string fileName = $"map_{map.mapid}_v{map.sceneVersion}.bundle";
        string filePath = Path.Combine(mapsDirectory, fileName);
        
        // Delete any older versions of this map
        CleanupOldMapVersions(map.mapid, map.sceneVersion);
        
        // Use a regular UnityWebRequest to download the file as bytes
        using (UnityWebRequest www = UnityWebRequest.Get(map.sceneLink))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // Save the raw file
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
            // Find all versions of this map
            string[] files = Directory.GetFiles(mapsDirectory, $"map_{mapId}_v*.bundle");
            
            foreach (string file in files)
            {
                // Skip if this is the current version
                if (file.Contains($"map_{mapId}_v{currentVersion}.bundle"))
                    continue;
                    
                // Delete old version
                File.Delete(file);
                Debug.Log($"Deleted old map version: {Path.GetFileName(file)}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error cleaning up old map versions: {ex.Message}");
        }
    }

    void Update()
    {
        
    }
    
    private void LoadMap(int mapId)
    {
        Debug.Log($"Loading map with ID: {mapId}");
        
        // Find the map data for this ID
        MapData mapToLoad = maps.Find(m => m.mapid == mapId);
        if (mapToLoad == null)
        {
            Debug.LogError($"Could not find map data for ID: {mapId}");
            return;
        }
        
        // Check if the map file exists locally
        string fileName = $"map_{mapId}_v{mapToLoad.sceneVersion}.bundle";
        string filePath = Path.Combine(Application.persistentDataPath, "Maps", fileName);
        
        if (File.Exists(filePath))
        {
            Debug.Log($"Loading map file from: {filePath}");
            StartCoroutine(LoadAssetBundleFromFile(filePath, mapToLoad));
        }
        else
        {
            Debug.LogError($"Map file not found at: {filePath}");
            // Try to download the map again
            StartCoroutine(DownloadMapFile(mapToLoad));
        }
    }

    private IEnumerator LoadAssetBundleFromFile(string filePath, MapData map)
    {
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(filePath);
        yield return request;

        AssetBundle bundle = request.assetBundle;
        if (bundle == null)
        {
            Debug.LogError($"Failed to load AssetBundle from path: {filePath}");
            yield break;
        }

        try
        {
            // Get all scene paths in the bundle
            string[] scenePaths = bundle.GetAllScenePaths();
            if (scenePaths.Length > 0)
            {
                // Load the first scene additively
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePaths[0]);
                Debug.Log($"Loading scene: {sceneName}");
                PlayerPrefs.SetString("MapSceneName", sceneName);
                PlayerPrefs.SetInt("MapID", map.mapid);
                yield return SceneManager.LoadSceneAsync(sceneName);
                Debug.Log($"Scene loaded successfully: {sceneName}");
            }
            else
            {
                // The bundle might contain other assets (models, prefabs, etc.)
                string[] assetNames = bundle.GetAllAssetNames();
                if (assetNames.Length > 0)
                {
                    Debug.Log($"Assets in bundle: {string.Join(", ", assetNames)}");
                    
                    // Instantiate the first asset (if it's a GameObject)
                    GameObject prefab = bundle.LoadAsset<GameObject>(assetNames[0]);
                    if (prefab != null)
                    {
                        Instantiate(prefab);
                        Debug.Log($"Instantiated asset: {assetNames[0]}");
                    }
                }
                else
                {
                    Debug.LogError("No assets found in bundle");
                }
            }
        }
        finally
        {
            // Unload the bundle but keep instantiated objects
            bundle.Unload(false);
        }
    }
}
