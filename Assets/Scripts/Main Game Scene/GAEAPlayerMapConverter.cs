using UnityEngine;
using System.Collections.Generic;

public class GAEAPlayerMapConverter : MonoBehaviour
{
    [Header("Conversion Settings")]
    public int sampleResolution = 64;
    public float heightThreshold = 0.1f;
    
    public List<GameObject> ConvertGAEAToPlayerMap(GAEATerrainManager gaeaTerrain, Transform mapParent, Vector2 tableSize)
    {
        List<GameObject> playerMapTiles = new List<GameObject>();
        
        if (!gaeaTerrain.HasTerrain())
        {
            Debug.LogError("No GAEA terrain available for conversion");
            return playerMapTiles;
        }
        
        Vector3 terrainSize = gaeaTerrain.GetTerrainSize();
        Vector3 terrainPos = gaeaTerrain.GetTerrainPosition();
        
        // Calculate sampling grid
        float stepX = terrainSize.x / sampleResolution;
        float stepZ = terrainSize.z / sampleResolution;
        
        // Create 2D representation
        TerrainSample[,] samples = new TerrainSample[sampleResolution, sampleResolution];
        
        for (int x = 0; x < sampleResolution; x++)
        {
            for (int z = 0; z < sampleResolution; z++)
            {
                Vector3 worldPos = terrainPos + new Vector3(x * stepX, 0, z * stepZ);
                
                samples[x, z] = new TerrainSample
                {
                    height = gaeaTerrain.GetHeightAtWorldPosition(worldPos),
                    terrainType = gaeaTerrain.GetTerrainTypeAtWorldPosition(worldPos),
                    worldPosition = worldPos
                };
            }
        }
        
        // Convert to 2D tiles
        playerMapTiles = CreatePlayerTilesFromSamples(samples, mapParent, tableSize);
        
        return playerMapTiles;
    }
    
    private List<GameObject> CreatePlayerTilesFromSamples(TerrainSample[,] samples, Transform mapParent, Vector2 tableSize)
    {
        List<GameObject> tiles = new List<GameObject>();
        
        int width = samples.GetLength(0);
        int height = samples.GetLength(1);
        
        // Calculate tile size to fit table
        float tileSize = Mathf.Min(tableSize.x / width, tableSize.y / height);
        
        // Center the map
        Vector3 startPos = mapParent.position - new Vector3(
            (width - 1) * tileSize * 0.5f,
            0,
            (height - 1) * tileSize * 0.5f
        );
        
        GameObject tilePrefab = Resources.Load<GameObject>("TerrainTilePrefab");
        if (tilePrefab == null)
        {
            Debug.LogError("TerrainTilePrefab not found in Resources");
            return tiles;
        }
        
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                TerrainSample sample = samples[x, z];
                
                Vector3 tilePos = startPos + new Vector3(x * tileSize, 0, z * tileSize);
                GameObject tile = Instantiate(tilePrefab, tilePos, Quaternion.identity, mapParent);
                
                // Scale tile
                tile.transform.localScale = Vector3.one * tileSize;
                
                // Set color based on terrain type
                Renderer renderer = tile.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = GetColorForTerrainType(sample.terrainType);
                }
                
                // Add height variation for visual depth
                float heightVariation = Mathf.Clamp01(sample.height / 50f) * 0.1f;
                tile.transform.position += Vector3.up * heightVariation;
                
                tiles.Add(tile);
            }
        }
        
        return tiles;
    }
    
    private Color GetColorForTerrainType(TerrainTile.TerrainType terrainType)
    {
        return terrainType switch
        {
            TerrainTile.TerrainType.Grass => new Color(0.2f, 0.8f, 0.2f),
            TerrainTile.TerrainType.Sand => new Color(0.9f, 0.8f, 0.5f),
            TerrainTile.TerrainType.Water => new Color(0.1f, 0.4f, 0.8f),
            TerrainTile.TerrainType.Rock => new Color(0.6f, 0.6f, 0.6f),
            TerrainTile.TerrainType.Forest => new Color(0.0f, 0.4f, 0.0f),
            TerrainTile.TerrainType.Hill => new Color(0.3f, 0.6f, 0.3f),
            TerrainTile.TerrainType.Snow => new Color(0.9f, 0.9f, 1.0f),
            _ => Color.gray
        };
    }
    
    private struct TerrainSample
    {
        public float height;
        public TerrainTile.TerrainType terrainType;
        public Vector3 worldPosition;
    }
}