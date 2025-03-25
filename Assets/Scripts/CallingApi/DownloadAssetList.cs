using System;
using System.Collections;
using System.Collections.Generic;
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

    public GameObject[] Zones;
    void Start()
    {
        StartCoroutine(GetBuildingsByMapId("1", buildings =>
        {
            foreach (var building in buildings)
            {
                if (!string.IsNullOrEmpty(building.asset_data_url))
                {
                    Debug.Log($"Loading {building.building_name} from {building.asset_data_url}");
                    StartCoroutine(DownloadAndLoadAssetBundle(building));
                }
            }
        }));
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
                    callback(new List<Building>());
                }
            }
            else
            {
                Debug.LogError($"Error retrieving buildings: {www.error}");
                callback(new List<Building>());
            }
        }
    }

    private IEnumerator DownloadAndLoadAssetBundle(Building building)
    {
        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(building.asset_data_url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to download asset bundle for {building.building_name}: {www.error}");
                yield break;
            }

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            if (bundle == null)
            {
                Debug.LogError($"Failed to load AssetBundle for {building.building_name}");
                yield break;
            }

            GameObject obj = null;
            yield return StartCoroutine(ProcessAssetBundle(bundle, building, (result) => obj = result));
            
            bundle.Unload(false);

            // Add BuildingSceneLoader component if scene_data_url exists
            if (!string.IsNullOrEmpty(building.scene_data_url))
            {
                Debug.Log("building.scene_data_url: " + building.scene_data_url);
                BuildingSceneLoader sceneLoader = obj.AddComponent<BuildingSceneLoader>();
                sceneLoader.sceneAssetBundleUrl = building.scene_data_url;
                
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
}