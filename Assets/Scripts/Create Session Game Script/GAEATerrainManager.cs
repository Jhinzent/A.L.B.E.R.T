using UnityEngine;
using System.Collections.Generic;

public class GAEATerrainManager : MonoBehaviour
{
    [Header("GAEA Terrain Settings")]
    public Terrain currentTerrain;
    public TerrainData currentTerrainData;
    
    private Vector3 terrainSize;
    private Vector3 terrainPosition;
    
    public bool LoadGAEATerrain(string terrainPath, Vector3 scale, Vector3 offset)
    {
        ClearCurrentTerrain();
        
        GameObject terrainPrefab = Resources.Load<GameObject>(terrainPath);
        if (terrainPrefab == null)
        {
            Debug.LogError($"Failed to load GAEA terrain from Resources: {terrainPath}");
            return false;
        }
        
        GameObject terrainInstance = Instantiate(terrainPrefab);
        terrainInstance.transform.position = offset;
        terrainInstance.transform.localScale = scale;
        
        currentTerrain = terrainInstance.GetComponent<Terrain>();
        if (currentTerrain == null)
        {
            Debug.LogError("Loaded prefab does not contain a Terrain component");
            Destroy(terrainInstance);
            return false;
        }
        
        currentTerrainData = currentTerrain.terrainData;
        terrainSize = currentTerrainData.size;
        terrainPosition = currentTerrain.transform.position;
        
        Debug.Log($"GAEA terrain loaded successfully. Size: {terrainSize}, Position: {terrainPosition}");
        return true;
    }
    
    public void ClearCurrentTerrain()
    {
        if (currentTerrain != null)
        {
            DestroyImmediate(currentTerrain.gameObject);
            currentTerrain = null;
            currentTerrainData = null;
        }
    }
    
    public float GetHeightAtWorldPosition(Vector3 worldPos)
    {
        if (currentTerrain == null) return 0f;
        
        Vector3 terrainLocalPos = worldPos - terrainPosition;
        Vector3 normalizedPos = new Vector3(
            terrainLocalPos.x / terrainSize.x,
            0,
            terrainLocalPos.z / terrainSize.z
        );
        
        return currentTerrain.SampleHeight(worldPos);
    }
    
    public TerrainTile.TerrainType GetTerrainTypeAtWorldPosition(Vector3 worldPos)
    {
        if (currentTerrainData == null) return TerrainTile.TerrainType.Grass;
        
        Vector3 terrainLocalPos = worldPos - terrainPosition;
        Vector3 normalizedPos = new Vector3(
            Mathf.Clamp01(terrainLocalPos.x / terrainSize.x),
            0,
            Mathf.Clamp01(terrainLocalPos.z / terrainSize.z)
        );
        
        int mapX = Mathf.FloorToInt(normalizedPos.x * currentTerrainData.alphamapWidth);
        int mapZ = Mathf.FloorToInt(normalizedPos.z * currentTerrainData.alphamapHeight);
        
        float[,,] splatmapData = currentTerrainData.GetAlphamaps(mapX, mapZ, 1, 1);
        
        // Find dominant texture
        float maxWeight = 0f;
        int dominantTexture = 0;
        
        for (int i = 0; i < splatmapData.GetLength(2); i++)
        {
            if (splatmapData[0, 0, i] > maxWeight)
            {
                maxWeight = splatmapData[0, 0, i];
                dominantTexture = i;
            }
        }
        
        return ConvertTextureIndexToTerrainType(dominantTexture);
    }
    
    private TerrainTile.TerrainType ConvertTextureIndexToTerrainType(int textureIndex)
    {
        // Map GAEA texture indices to your terrain types
        // This mapping should be configured based on your GAEA terrain setup
        return textureIndex switch
        {
            0 => TerrainTile.TerrainType.Grass,
            1 => TerrainTile.TerrainType.Rock,
            2 => TerrainTile.TerrainType.Sand,
            3 => TerrainTile.TerrainType.Water,
            4 => TerrainTile.TerrainType.Forest,
            _ => TerrainTile.TerrainType.Grass
        };
    }
    
    public Vector3 GetTerrainSize() => terrainSize;
    public Vector3 GetTerrainPosition() => terrainPosition;
    public bool HasTerrain() => currentTerrain != null;
}