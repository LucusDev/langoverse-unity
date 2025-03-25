using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class BuildingController : MonoBehaviour
{
    [SerializeField] public GameObject langoMap; // Assign LangoMap in the Inspector

    // Building to replace & its AssetBundle URL
    private string targetBuildingID = "Airplane"; // Change this to your target building
    private string assetBundleUrl = "https://drive.google.com/uc?export=download&id=1HOd7VQhR619ws-kWdCM2mFX6KrM1LTOj";

    void Start()
    {
        if (langoMap == null)
        {
            Debug.LogError("LangoMap GameObject is not assigned!");
            return;
        }

        StartCoroutine(CheckAndReplaceBuilding());
    }

    /// <summary>
    /// Checks if the building exists and replaces it.
    /// </summary>
    IEnumerator CheckAndReplaceBuilding()
    {
        Transform targetBuilding = null;

        // Loop through all buildings in LangoMap
        foreach (Transform building in langoMap.transform)
        {
            string buildingID = building.gameObject.name; // Using GameObject name as ID

            if (buildingID == targetBuildingID)
            {
                targetBuilding = building;
                break; // Stop searching once found
            }
        }

        if (targetBuilding == null)
        {
            Debug.LogWarning($"Building with ID '{targetBuildingID}' not found!");
            yield break;
        }

        Debug.Log($"Found building '{targetBuildingID}', replacing...");

        // Get building's position & rotation before destroying
        Vector3 position = targetBuilding.position;
        Quaternion rotation = Quaternion.identity;
     

        // Destroy old building
        Destroy(targetBuilding.gameObject);

        // Download and replace with AssetBundle
        yield return StartCoroutine(DownloadAndReplaceBuilding(position, rotation));
    }

    /// <summary>
    /// Downloads the AssetBundle and replaces the building.
    /// </summary>
    IEnumerator DownloadAndReplaceBuilding(Vector3 position, Quaternion rotation)
    {
        using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(assetBundleUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download AssetBundle: " + request.error);
                yield break;
            }

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
            if (bundle == null)
            {
                Debug.LogError("Failed to load AssetBundle.");
                yield break;
            }

            // Get the first asset in the bundle
            string[] assetNames = bundle.GetAllAssetNames();
            if (assetNames.Length == 0)
            {
                Debug.LogError("No assets found in AssetBundle.");
                bundle.Unload(false);
                yield break;
            }

            GameObject newBuildingPrefab = bundle.LoadAsset<GameObject>(assetNames[0]);
            if (newBuildingPrefab == null)
            {
                Debug.LogError("Failed to load building from AssetBundle.");
                bundle.Unload(false);
                yield break;
            }

            // Instantiate new building
            GameObject newBuilding = Instantiate(newBuildingPrefab, position, rotation);
            newBuilding.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            newBuilding.name = targetBuildingID; // Keep same name for reference

            Debug.Log($"Successfully replaced '{targetBuildingID}' with new building.");

            // Unload AssetBundle but keep the loaded asset
            bundle.Unload(false);
        }
    }
}
