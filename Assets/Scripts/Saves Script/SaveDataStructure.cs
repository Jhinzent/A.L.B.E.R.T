using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectData
{
    public string objectName;
    public string objectGivenName;
    public Vector3 position;
    public Quaternion rotation;
    // public string terrainType;  // Commented out - old tile system
    public string prefabName;
    public PlaceableItem.ItemType itemType;
    public string team;
    
    // New attributes for units
    public int proficiency = 1;
    public int fatigue = 1;
    public int commsClarity = 1;
    public int equipment = 1;
}

[System.Serializable]
public class SaveData
{
    public string saveName;
    public List<ObjectData> objects = new List<ObjectData>();
    public GAEAMapData gaeaMapData;
}

[System.Serializable]
public class GAEAMapData
{
    public string imagePath;
    public string objPath;
    public Vector3 mapPosition;
    public Vector3 mapScale;
}