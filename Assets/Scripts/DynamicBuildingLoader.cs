using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;

public class DynamicBuildingLoader : MonoBehaviour
{
    [SerializeField] private TestJsonData testJsonData;

    private BuildingList buildingList;
    public Transform parentTransform;
    public GameObject loadingScreen;
    public Slider progressBar;

    private int loadedBuildings = 0;
    private int totalBuildings = 0;

    void OnEnable()
    {
        Debug.Log("- Fetching Data -");
        StartCoroutine(FetchBuildingsData());
    }

    IEnumerator FetchBuildingsData()
    {
        // Show loading screen
        loadingScreen.SetActive(true);
        progressBar.value = 0f;

        // // ✅ Check if buildings are already cached in memory
        // if (BuildingCache.cachedBuildings.Count > 0)
        // {
        //     Debug.Log("Loading buildings from memory cache...");
        //     foreach (var building in BuildingCache.cachedBuildings)
        //     {
        //         StartCoroutine(LoadBuilding(building));
        //     }
        //     loadingScreen.SetActive(false);
        //     progressBar.value = 1f;
        //     yield break;
        // }

        // ✅ Fetch building list from local JSON
        buildingList = testJsonData.GetBuildingList();

        totalBuildings = buildingList.buildings.Count;
        foreach (BuildingInfo buildingInfo in buildingList.buildings)
        {
            StartCoroutine(LoadBuilding(buildingInfo));
        }

        // Wait until all buildings are loaded
        while (loadedBuildings < totalBuildings)
        {
            yield return null;
        }

        // Hide loading screen
        loadingScreen.SetActive(false);
        Debug.Log("All Buildings Loaded!");
    }
    IEnumerator LoadBuilding(BuildingInfo buildingInfo)
    {
        string bundleName = buildingInfo.building_id;
        string assetUrl = buildingInfo.asset_url;
        
        if (PermanentAssetBundleManager.Instance == null)
        {
            Debug.LogError("PermanentAssetBundleManager instance is not available.");
            yield break;
        }

        // ✅ Check if AssetBundle is already loaded
        AssetBundle bundle = PermanentAssetBundleManager.Instance.GetBundle(bundleName);

        if (bundle == null)
        {
            // ✅ If not in memory, download it
            yield return PermanentAssetBundleManager.Instance.DownloadAndCacheBundle(bundleName, assetUrl);
            bundle = PermanentAssetBundleManager.Instance.GetBundle(bundleName);
        }

        Debug.Log($"BUNDLE : {bundle}");

        if (bundle != null)
        {
            string[] assetNames = bundle.GetAllAssetNames();
            GameObject prefab = bundle.LoadAsset<GameObject>(assetNames[0]);

            if (prefab != null)
            {
                InstantiateBuilding(prefab, buildingInfo);
            }
            else
            {
                Debug.LogError("Prefab not found in bundle: " + buildingInfo.building_name);
            }
        }
        else
        {
            Debug.LogError("Failed to load AssetBundle for " + buildingInfo.building_name);
        }
        // ✅ Increase the loadedBuildings counter
        loadedBuildings++;

        // ✅ Update progress bar
        progressBar.value = (float)loadedBuildings / totalBuildings;
    }
    private void InstantiateBuilding(GameObject prefab, BuildingInfo buildingInfo)
    {
        GameObject obj = Instantiate(prefab);
        obj.transform.position = new Vector3(buildingInfo.position.x, buildingInfo.position.y, buildingInfo.position.z);
        obj.transform.rotation = Quaternion.Euler(buildingInfo.rotation.x, buildingInfo.rotation.y, buildingInfo.rotation.z);
        obj.name = buildingInfo.building_name;

        // ✅ Set to parent (optional)
        obj.transform.SetParent(null);

        // ✅ Add Collider for Click Detection
        if (obj.GetComponent<Collider>() == null)
        {
            BoxCollider collider = obj.AddComponent<BoxCollider>();
            collider.size = new Vector3(5, 5, 5);
        }

        // ✅ Add Interaction Script
        BuildingInteraction interaction = obj.AddComponent<BuildingInteraction>();
        interaction.buildingData = buildingInfo;

    }
}
