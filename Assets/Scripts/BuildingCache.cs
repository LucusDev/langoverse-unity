using System.Collections.Generic;
using UnityEngine;

public static class BuildingCache
{
    public static List<BuildingInfo> cachedBuildings = new List<BuildingInfo>();
    public static Dictionary<string, GameObject> loadedBuildings = new Dictionary<string, GameObject>();
}

