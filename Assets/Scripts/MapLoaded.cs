using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLoaded : MonoBehaviour
{
  [SerializeField]  public GameObject langoMap; // Assign LangoMap in the Inspector

    void Start()
    {
        if (langoMap == null)
        {
            Debug.LogError("LangoMap GameObject is not assigned!");
            return;
        }

        foreach (Transform building in langoMap.transform)
        {
            string buildingID = building.gameObject.name; // Using GameObject name as ID
            string buildingName = building.gameObject.name; // Change this if you have a custom name system
            Vector3 position = building.position;

            Debug.Log($"Building ID: {buildingID}, Name: {buildingName}, Position: {position}");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
