using UnityEngine;
using System.Collections.Generic;

public class UnifiedMapManager : MonoBehaviour
{
    [Header("Map Components")]
    public GameMasterMapManager tileBased;
    public GAEATerrainManager gaeaTerrain;
    
    [Header("Current Map Settings")]
    public MapConfiguration currentMapConfig;
    
    private MapType currentMapType = MapType.TileBased;
    
    public void InitializeMap(MapConfiguration config)
    {
        currentMapConfig = config;
        currentMapType = config.mapType;
        
        switch (config.mapType)
        {
            case MapType.TileBased:
                InitializeTileBasedMap();
                break;
            case MapType.GAEATerrain:
                InitializeGAEAMap(config);
                break;
        }
    }
    
    private void InitializeTileBasedMap()
    {
        if (gaeaTerrain != null)
            gaeaTerrain.ClearCurrentTerrain();
        
        if (tileBased != null)
            tileBased.GenerateGrid();
    }
    
    private void InitializeGAEAMap(MapConfiguration config)
    {
        if (gaeaTerrain != null)
        {
            bool success = gaeaTerrain.LoadGAEATerrain(
                config.gaeaTerrainPath, 
                config.terrainScale, 
                config.terrainOffset
            );
            
            if (!success)
            {
                Debug.LogError("Failed to load GAEA terrain, falling back to tile-based");
                currentMapType = MapType.TileBased;
                InitializeTileBasedMap();
            }
        }
    }
    
    // COMMENTED OUT - OLD TILE SYSTEM
    /*
    public void LoadMapFromSave(SaveData saveData)
    {
        if (saveData.mapConfig != null)
        {
            InitializeMap(saveData.mapConfig);
        }
        else
        {
            // Legacy save file - assume tile-based
            currentMapConfig = new MapConfiguration { mapType = MapType.TileBased };
            InitializeTileBasedMap();
        }
        
        // Load objects regardless of map type
        LoadPlaceableObjects(saveData.objects);
    }
    
    private void LoadPlaceableObjects(List<ObjectData> objects)
    {
        foreach (ObjectData obj in objects)
        {
            // Skip terrain tiles for GAEA maps
            if (currentMapType == MapType.GAEATerrain && !string.IsNullOrEmpty(obj.terrainType))
                continue;
                
            // Load other objects (units, items, etc.)
            LoadObject(obj);
        }
    }
    */
    
    private void LoadObject(ObjectData obj)
    {
        // Implementation depends on your existing object loading system
        // This should integrate with your PlaceableItemInstance system
        Debug.Log($"Loading object: {obj.objectName} at {obj.position}");
    }
    
    public float GetHeightAtPosition(Vector3 worldPos)
    {
        return currentMapType switch
        {
            MapType.GAEATerrain => gaeaTerrain?.GetHeightAtWorldPosition(worldPos) ?? 0f,
            MapType.TileBased => GetTileHeightAtPosition(worldPos),
            _ => 0f
        };
    }
    
    private float GetTileHeightAtPosition(Vector3 worldPos)
    {
        if (tileBased == null) return 0f;
        
        TerrainTile tile = tileBased.GetTileAtWorldPosition(worldPos);
        return tile != null ? tile.transform.position.y : 0f;
    }
    
    public TerrainTile.TerrainType GetTerrainTypeAtPosition(Vector3 worldPos)
    {
        return currentMapType switch
        {
            MapType.GAEATerrain => gaeaTerrain?.GetTerrainTypeAtWorldPosition(worldPos) ?? TerrainTile.TerrainType.Grass,
            MapType.TileBased => tileBased?.GetTileAtWorldPosition(worldPos)?.GetTerrainType() ?? TerrainTile.TerrainType.None,
            _ => TerrainTile.TerrainType.None
        };
    }
    
    public MapType GetCurrentMapType() => currentMapType;
    public MapConfiguration GetCurrentMapConfig() => currentMapConfig;
}