using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class TestJsonData : MonoBehaviour
{
    [SerializeField]
    public string jsonData = @"
    {
        'buildings': [
            {
                'building_name': 'industrialprefab',
                'building_id': 'A1',
                'asset_url': 'https://drive.google.com/uc?export=download&id=1G4a8lt_jlvwHHhpqMVr6ORFDR4m978Au',
                'position': { 'x': -10, 'y': 4.45, 'z': -35 },
                'rotation': { 'x': 0, 'y': 90, 'z': 0 }
            },
            {
                'building_name': 'govermentprefab',
                'building_id': 'B1',
                'asset_url': 'https://drive.google.com/uc?export=download&id=1gytlHmZnFMDjhMxkO8Lh_JPAuVtOsFCD',
                'position': { 'x': -10, 'y': 4.45, 'z': 25 },
                'rotation': { 'x': 0, 'y': 180, 'z': 0 }
            },
            {
                'building_name': 'constructionprefab',
                'building_id': 'B3',
                'asset_url': 'https://drive.google.com/uc?export=download&id=1Z_TwSp2t2QXjE51oToEi3hlmieA7v417',
                'position': { 'x': 68.5, 'y': 4.45, 'z': 15 },
                'rotation': { 'x': 0, 'y': 0, 'z': 0 },
                'isHaveInterior': true,
                'interior_scene_url': '1HOd7VQhR619ws-kWdCM2mFX6KrM1LTOj'
            }
        ]
    }";

    public BuildingList GetBuildingList()
    {
        try
        {
            return JsonConvert.DeserializeObject<BuildingList>(jsonData);
        }
        catch (JsonException ex)
        {
            Debug.LogError($"Failed to deserialize JSON data: {ex.Message}");
            return null;
        }
    }
}

[System.Serializable]
public class BuildingList
{
    public List<BuildingInfo> buildings;
}

[System.Serializable]
public class BuildingInfo
{
    public string building_name;
    public string building_id;
    public string asset_url;
    public BuildingSpec position;
    public BuildingSpec rotation;

    public bool isHaveInterior;

    public string interior_scene_url;


}

[System.Serializable]
public class BuildingSpec
{
    public float x;
    public float y;
    public float z;
}