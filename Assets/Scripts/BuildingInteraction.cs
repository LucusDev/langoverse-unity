using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingInteraction : MonoBehaviour
{
    public BuildingInfo buildingData;

    void OnMouseDown()
    {
        Debug.Log($"Tapped on {buildingData.building_name}");

        if (buildingData.isHaveInterior)
        {
            Debug.Log($"Loading Interior Scene from: {buildingData.interior_scene_url}");
            LoadInterior(buildingData.interior_scene_url);
        }
    }

    void LoadInterior(string sceneUrl)
    {
        Debug.Log("Interior Scene would be loaded from: " + sceneUrl);
        PlayerPrefs.SetString("InteriorSceneBundleId", sceneUrl);
        SceneManager.LoadScene("InteriorScene");
    }
}
