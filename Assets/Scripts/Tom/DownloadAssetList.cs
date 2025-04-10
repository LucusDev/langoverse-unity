using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class DownloadAssetList : MonoBehaviour
{
    private string supabaseUrl = "https://nzrlskengaeocyqxgkug.supabase.co";
    private string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im56cmxza2VuZ2Flb2N5cXhna3VnIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDI1Mzg0NTYsImV4cCI6MjA1ODExNDQ1Nn0.aPqgdGBFbwm7R0G3zzUceopLSrbZ-1fY1ikdK9miRmU"; // Hide API keys in production!

    [Serializable]
    public class Building
    {
        public string building_id;
        public string map_id;
        public string zone_id;
        public string building_name;
        public string asset_data_url;
        public int asset_version;
        public float rotation_x;
        public float rotation_y;
        public float rotation_z;
        public bool is_have_interior;
        public string scene_data_url;
        public int? scene_version;
    }

    private GameObject[] Zones;
    private List<Building> localBuildings = new List<Building>();
    
    private int mapId = 1;
    void Start()
    {
        // Load local buildings data
        LoadLocalBuildingsData();
        mapId = PlayerPrefs.GetInt("mapId");
        // Get buildings by map ID
        StartCoroutine(GetBuildingsByMapId(mapId.ToString(), buildings =>
        {
            // Assign Zones based on the number of buildings
            AssignZones(buildings.Count);
            
            SaveBuildingsDataToJson(buildings);
            CheckAndDownloadBuildings(buildings);
        }));
    }

    private void LoadLocalBuildingsData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "buildings_data.json");
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                localBuildings = JsonConvert.DeserializeObject<List<Building>>(json);
                Debug.Log($"Loaded {localBuildings.Count} buildings from local storage");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load local buildings data: {ex.Message}");
                localBuildings = new List<Building>();
            }
        }
    }

    private void SaveBuildingsDataToJson(List<Building> buildings)
    {
        try
        {
            string json = JsonConvert.SerializeObject(buildings, Formatting.Indented);
            string filePath = Path.Combine(Application.persistentDataPath, "buildings_data.json");
            File.WriteAllText(filePath, json);
            Debug.Log($"Buildings data saved to: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save buildings data to JSON: {ex.Message}");
        }
    }

    private void CheckAndDownloadBuildings(List<Building> buildings)
        {
            foreach (var building in buildings)
            {
            if (string.IsNullOrEmpty(building.asset_data_url))
            {
                continue;
            }

            bool needsAssetDownload = true;
            bool needsSceneDownload = !string.IsNullOrEmpty(building.scene_data_url);
            
            // Check if we have this building locally
            Building localBuilding = localBuildings.Find(b => b.building_id == building.building_id);
            
            // If we have the building locally, check asset version
            if (localBuilding != null)
            {
                // Check if the local asset version is the same as server version
                if (localBuilding.asset_version == building.asset_version)
                {
                    // Check if the file actually exists
                    string assetFileName = $"building_{building.building_id}_asset_v{building.asset_version}.bundle";
                    string assetFilePath = Path.Combine(Application.persistentDataPath, "Buildings", assetFileName);
                    
                    if (File.Exists(assetFilePath))
                    {
                        needsAssetDownload = false;
                        Debug.Log($"Building {building.building_name} asset is up to date (v{building.asset_version})");
                    }
                    else
                    {
                        Debug.Log($"Building {building.building_name} asset file missing locally, will download");
                    }
                }
                else
                {
                    Debug.Log($"Building {building.building_name} has a new asset version (local: v{localBuilding.asset_version}, server: v{building.asset_version})");
                }
                
                // Check scene version if scene data exists
                if (needsSceneDownload && building.scene_version.HasValue && localBuilding.scene_version.HasValue)
                {
                    if (localBuilding.scene_version.Value == building.scene_version.Value)
                    {
                        string sceneFileName = $"building_{building.building_id}_scene_v{building.scene_version}.bundle";
                        string sceneFilePath = Path.Combine(Application.persistentDataPath, "Buildings", sceneFileName);
                        
                        if (File.Exists(sceneFilePath))
                        {
                            needsSceneDownload = false;
                            Debug.Log($"Building {building.building_name} scene is up to date (v{building.scene_version})");
                        }
                        else
                        {
                            Debug.Log($"Building {building.building_name} scene file missing locally, will download");
                        }
                    }
                    else
                    {
                        Debug.Log($"Building {building.building_name} has a new scene version (local: v{localBuilding.scene_version}, server: v{building.scene_version})");
                    }
                }
            }
            else
            {
                Debug.Log($"New building found: {building.building_name} (asset v{building.asset_version}, scene v{building.scene_version})");
            }
            
            // Download and load if needed
            if (needsAssetDownload)
            {
                StartCoroutine(DownloadBuildingAsset(building));
            }
            else
            {
                // If asset is already downloaded, load it from cache
                StartCoroutine(LoadBuildingAssetFromCache(building));
            }
            
            // Handle scene if needed (can be improved to download only if needed)
            if (needsSceneDownload && !string.IsNullOrEmpty(building.scene_data_url))
            {
                StartCoroutine(DownloadBuildingScene(building));
            }
        }
    }

    private IEnumerator DownloadBuildingAsset(Building building)
    {
        // Create directory if it doesn't exist
        string buildingsDirectory = Path.Combine(Application.persistentDataPath, "Buildings");
        if (!Directory.Exists(buildingsDirectory))
        {
            Directory.CreateDirectory(buildingsDirectory);
        }

        string assetFileName = $"building_{building.building_id}_asset_v{building.asset_version}.bundle";
        string assetFilePath = Path.Combine(buildingsDirectory, assetFileName);
        
        // Delete any older versions of this building's asset
        CleanupOldBuildingVersions(building.building_id, "asset", building.asset_version);
        
        Debug.Log($"Downloading building asset from: {building.asset_data_url}");
        
        using (UnityWebRequest www = UnityWebRequest.Get(building.asset_data_url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // Save the raw file
                    File.WriteAllBytes(assetFilePath, www.downloadHandler.data);
                    Debug.Log($"Building asset for {building.building_name} saved to: {assetFilePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to save building asset for {building.building_name}: {ex.Message}");
                    yield break;
                }
                
                // Now load the downloaded asset - moved outside the try-catch block
                yield return StartCoroutine(LoadBuildingAssetFromCache(building));
            }
            else
            {
                Debug.LogError($"Error downloading building asset for {building.building_name}: {www.error}");
            }
        }
    }
    
    private IEnumerator DownloadBuildingScene(Building building)
    {
        if (!building.scene_version.HasValue)
        {
            Debug.LogError($"Scene version is null for building {building.building_name}");
            yield break;
        }
        
        // Create directory if it doesn't exist
        string buildingsDirectory = Path.Combine(Application.persistentDataPath, "Buildings");
        if (!Directory.Exists(buildingsDirectory))
        {
            Directory.CreateDirectory(buildingsDirectory);
        }

        string sceneFileName = $"building_{building.building_id}_scene_v{building.scene_version}.bundle";
        string sceneFilePath = Path.Combine(buildingsDirectory, sceneFileName);
        
        // Delete any older versions of this building's scene
        CleanupOldBuildingVersions(building.building_id, "scene", building.scene_version.Value);
        
        Debug.Log($"Downloading building scene from: {building.scene_data_url}");
        
        using (UnityWebRequest www = UnityWebRequest.Get(building.scene_data_url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // Save the raw file
                    File.WriteAllBytes(sceneFilePath, www.downloadHandler.data);
                    Debug.Log($"Building scene for {building.building_name} saved to: {sceneFilePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to save building scene for {building.building_name}: {ex.Message}");
                }
            }
            else
            {
                Debug.LogError($"Error downloading building scene for {building.building_name}: {www.error}");
            }
        }
    }

    private IEnumerator LoadBuildingAssetFromCache(Building building)
    {
        string assetFileName = $"building_{building.building_id}_asset_v{building.asset_version}.bundle";
        string assetFilePath = Path.Combine(Application.persistentDataPath, "Buildings", assetFileName);
        
        if (!File.Exists(assetFilePath))
        {
            Debug.LogError($"Building asset file not found at: {assetFilePath}");
            yield break;
        }
        
        Debug.Log($"Loading building asset from cache: {assetFilePath}");
        
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(assetFilePath);
        yield return request;

        AssetBundle bundle = request.assetBundle;
        if (bundle == null)
        {
            Debug.LogError($"Failed to load AssetBundle for {building.building_name} from cache");
            yield break;
        }

        GameObject obj = null;
        yield return StartCoroutine(ProcessAssetBundle(bundle, building, (result) => obj = result));
        
        bundle.Unload(false);

        // Add BuildingSceneLoader component if scene_data_url exists
        if (!string.IsNullOrEmpty(building.scene_data_url))
        {
            // Instead of using the URL directly, point to the cached scene file
            BuildingSceneLoader sceneLoader = obj.AddComponent<BuildingSceneLoader>();
            
            if (building.scene_version.HasValue)
            {
                string sceneFileName = $"building_{building.building_id}_scene_v{building.scene_version}.bundle";
                string sceneFilePath = Path.Combine(Application.persistentDataPath, "Buildings", sceneFileName);
                
                // Use the cached file path if it exists, otherwise use the URL
                if (File.Exists(sceneFilePath))
                {
                    sceneLoader.sceneAssetBundleUrl = "file://" + sceneFilePath;
                    Debug.Log($"Using cached scene for {building.building_name}: {sceneFilePath}");
                }
                else
                {
                    sceneLoader.sceneAssetBundleUrl = building.scene_data_url;
                    Debug.Log($"Using remote scene URL for {building.building_name}");
                }
            }
            else
            {
                sceneLoader.sceneAssetBundleUrl = building.scene_data_url;
            }
            
            // Add a collider for click detection
            BoxCollider collider = obj.AddComponent<BoxCollider>();
            // Automatically size the collider to fit the object
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                collider.size = renderer.bounds.size;
                collider.center = renderer.bounds.center;
            }
        }
    }

    private void CleanupOldBuildingVersions(string buildingId, string assetType, int currentVersion)
    {
        string buildingsDirectory = Path.Combine(Application.persistentDataPath, "Buildings");
        if (!Directory.Exists(buildingsDirectory))
            return;
            
        try
        {
            // Find all versions of this building's assets
            string[] files = Directory.GetFiles(buildingsDirectory, $"building_{buildingId}_{assetType}_v*.bundle");
            
            foreach (string file in files)
            {
                // Skip if this is the current version
                if (file.Contains($"building_{buildingId}_{assetType}_v{currentVersion}.bundle"))
                    continue;
                    
                // Delete old version
                File.Delete(file);
                Debug.Log($"Deleted old building {assetType} version: {Path.GetFileName(file)}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error cleaning up old building versions: {ex.Message}");
        }
    }

    public IEnumerator GetBuildingsByMapId(string mapId, Action<List<Building>> callback)
    {
        string endpoint = $"{supabaseUrl}/rest/v1/buildings?map_id=eq.{mapId}";

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
                    List<Building> buildings = JsonConvert.DeserializeObject<List<Building>>(responseText);
                    Debug.Log($"Retrieved {buildings.Count} buildings for map ID {mapId}");
                    callback(buildings);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to parse buildings data: {ex.Message}");
                    
                    // If we have local buildings data, use that instead
                    if (localBuildings.Count > 0)
                    {
                        Debug.Log("Using cached building data");
                        var mapBuildings = localBuildings.FindAll(b => b.map_id == mapId);
                        callback(mapBuildings);
            }
            else
            {
                callback(new List<Building>());
            }
        }
    }
            else
            {
                Debug.LogError($"Error retrieving buildings: {www.error}");
                
                // If we have local buildings data, use that instead
                if (localBuildings.Count > 0)
                {
                    Debug.Log("Using cached building data");
                    var mapBuildings = localBuildings.FindAll(b => b.map_id == mapId);
                    callback(mapBuildings);
                }
                else
                {
                    callback(new List<Building>());
                }
            }
        }
    }

    private IEnumerator ProcessAssetBundle(AssetBundle bundle, Building building, Action<GameObject> callback)
    {
        string[] assetNames = bundle.GetAllAssetNames();
        Debug.Log($"Assets in bundle ({building.building_name}): {string.Join(", ", assetNames)}");

        if (assetNames.Length == 0)
        {
            Debug.LogError($"No assets found in AssetBundle for {building.building_name}");
            callback(null);
            yield break;
        }

        UnityEngine.Object asset = bundle.LoadAsset(assetNames[0]);
        if (asset == null)
        {
            Debug.LogError($"Failed to load asset from bundle for {building.building_name}");
            callback(null);
            yield break;
        }

        GameObject obj = ProcessAsset(asset, building);
        if (obj != null)
        {
            obj.transform.rotation = Quaternion.Euler(building.rotation_x, building.rotation_y, building.rotation_z);
            Debug.Log($"Successfully instantiated {building.building_name}");
        }

        callback(obj);
        yield return null;
    }

    private GameObject ProcessAsset(UnityEngine.Object asset, Building building)
    {
        GameObject obj = null;
        if (asset is Mesh mesh)
        {
            obj = CreateMeshGameObject(mesh, building.building_name);
        }
        else if (asset is GameObject prefab)
        {
            obj = InstantiatePrefab(prefab, building.building_name);
        }
        else
        {
            Debug.LogError($"Unsupported asset type in bundle: {asset.GetType()}");
            return null;
        }

        // Set parent based on zone_id
        if (obj != null && !string.IsNullOrEmpty(building.zone_id))
        {
            if (int.TryParse(building.zone_id, out int zoneIndex) && zoneIndex > 0 && zoneIndex <= Zones.Length)
            {
                obj.transform.SetParent(Zones[zoneIndex - 1].transform, true);
                obj.transform.localPosition = Vector3.zero;
                Debug.Log($"Set {building.building_name} as child of Zone {zoneIndex}");
            }
            else
            {
                Debug.LogWarning($"Invalid zone_id {building.zone_id} for building {building.building_name}");
            }
        }

        return obj;
    }

    private GameObject CreateMeshGameObject(Mesh mesh, string name)
    {
        GameObject obj = new GameObject(name);
        obj.AddComponent<MeshFilter>().mesh = mesh;
        obj.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));

        Debug.Log($"Successfully loaded {name} as a Mesh");
        return obj;
    }

    private GameObject InstantiatePrefab(GameObject prefab, string name)
    {
        GameObject obj = Instantiate(prefab);
        obj.name = name;

        Debug.Log($"Successfully loaded {name} as a GameObject");
        return obj;
    }

    private void AssignZones(int buildingCount)
    {
        Zones = new GameObject[buildingCount];
        
        for (int i = 0; i < buildingCount; i++)
        {
            string zoneName = $"Zone{i + 1}"; // Construct the zone name
            Zones[i] = GameObject.Find(zoneName); // Find the GameObject by name
            if (Zones[i] == null)
            {
                Debug.LogWarning($"No GameObject found with name: {zoneName}");
            }
        }
    }
}