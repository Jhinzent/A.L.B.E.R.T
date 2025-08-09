using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectData
{
    public string objectName;
    public string objectGivenName;
    public Vector3 position;
    public Quaternion rotation;
    public string terrainType;
    public string prefabName;
    public PlaceableItem.ItemType itemType;
    public string team;
}

[System.Serializable]
public class SaveData
{
    public string saveName;
    public List<ObjectData> objects = new List<ObjectData>();
}