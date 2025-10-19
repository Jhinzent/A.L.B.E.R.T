using UnityEngine;

public enum MapType
{
    TileBased,
    GAEATerrain
}

[System.Serializable]
public class MapConfiguration
{
    public MapType mapType;
    public string gaeaTerrainPath; // Path to GAEA terrain file
    public Vector3 terrainScale = Vector3.one;
    public Vector3 terrainOffset = Vector3.zero;
}