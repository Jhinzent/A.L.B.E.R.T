using System.Collections.Generic;
using UnityEngine;

public class PlayerMapLoader : MonoBehaviour
{
    public GameObject terrainTilePrefab;
    public List<Transform> mapAnchors = new List<Transform>();
    public List<CameraMover> playerCameraMovers = new List<CameraMover>();
    public Transform mapParent;
    public Vector2 tableSize = new Vector2(1.75f, 1f);
    public string saveName;
    public GameMasterMapLoader gameMasterMapLoader;
    public GeneralSessionManager generalSessionManager;
    public UnifiedMapManager unifiedMapManager;
    public GAEAPlayerMapConverter gaeaConverter;

    private readonly List<List<GameObject>> playerMaps = new();

    public void CreatePlayerMaps(int numberOfPlayers)
    {
        if (numberOfPlayers <= 0 || numberOfPlayers > mapAnchors.Count)
        {
            Debug.LogError($"[PlayerMapLoader] Invalid number of players: {numberOfPlayers}. Available anchors: {mapAnchors.Count}");
            return;
        }

        saveName = generalSessionManager.getGameSessionSaveName();
        // Debug.Log($"[PlayerMapLoader] Creating {numberOfPlayers} player maps. Save name: {saveName}");

        DeleteAllMaps();
        
        playerMaps.Clear();
        for (int i = 0; i < numberOfPlayers; i++)
        {
            playerMaps.Add(new List<GameObject>());
        }

        SaveData data = SaveSystem.LoadSession(saveName);
        if (data != null)
        {
            // Debug.Log($"[PlayerMapLoader] Save data found. Creating maps for {numberOfPlayers} players.");
            for (int i = 0; i < numberOfPlayers; i++)
            {
                BuildMapForPlayer(data, i);
            }
        }
        else
        {
            Debug.LogWarning($"[PlayerMapLoader] No save data found for '{saveName}'");
        }
    }

    public void ReloadMaps()
    {
        if (playerMaps.Count == 0)
        {
            Debug.LogWarning("[PlayerMapLoader] No maps to reload. Call CreatePlayerMaps first.");
            return;
        }
        
        // Debug.Log($"[PlayerMapLoader] Reloading {playerMaps.Count} player maps...");
        CreatePlayerMaps(playerMaps.Count);
    }

    public void ReloadPlayerMapsFromSave(SaveData saveData, int numberOfPlayers)
    {
        if (numberOfPlayers <= 0 || numberOfPlayers > mapAnchors.Count)
        {
            Debug.LogError($"[PlayerMapLoader] Invalid number of players: {numberOfPlayers}. Available anchors: {mapAnchors.Count}");
            return;
        }

        DeleteAllMaps();
        
        playerMaps.Clear();
        for (int i = 0; i < numberOfPlayers; i++)
        {
            playerMaps.Add(new List<GameObject>());
        }

        for (int i = 0; i < numberOfPlayers; i++)
        {
            BuildMapForPlayer(saveData, i);
        }
    }

    private void DeleteAllMaps()
    {
        int totalTiles = 0;
        foreach (var playerTiles in playerMaps)
        {
            totalTiles += playerTiles.Count;
            foreach (GameObject tile in playerTiles)
            {
                if (tile != null)
                    Destroy(tile);
            }
        }
        // Debug.Log($"[PlayerMapLoader] Deleted {totalTiles} tiles from {playerMaps.Count} player maps.");
        playerMaps.Clear();
    }

    void BuildMapForPlayer(SaveData data, int playerIndex)
    {
        mapParent = mapAnchors[playerIndex];
        
        // Check if GAEA map data exists and build GAEA map
        if (data.gaeaMapData != null && !string.IsNullOrEmpty(data.gaeaMapData.imagePath) && !string.IsNullOrEmpty(data.gaeaMapData.objPath))
        {
            BuildGAEAMapForPlayer(data, playerIndex);
            return;
        }
        
        // Original tile-based map building
        List<ObjectData> terrainObjects = new();

        /*
        foreach (ObjectData obj in data.objects)
        {
            if (System.Enum.TryParse(obj.terrainType, out PlaceableItem.TerrainType terrainType))
            {
                terrainObjects.Add(obj);
            }
        }
        */

        // Debug.Log($"[PlayerMapLoader] Found {terrainObjects.Count} terrain objects.");

        if (terrainObjects.Count == 0)
        {
            Debug.LogWarning("[PlayerMapLoader] No terrain tiles to display.");
            return;
        }

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (ObjectData obj in terrainObjects)
        {
            Vector2 pos2D = new Vector2(obj.position.x, obj.position.z);
            min = Vector2.Min(min, pos2D);
            max = Vector2.Max(max, pos2D);
        }

        Vector2 mapSize = (max - min) + Vector2.one;
        // Debug.Log($"[PlayerMapLoader] Calculated map bounds: min={min}, max={max}, size={mapSize}");

        float targetAspect = tableSize.x / tableSize.y;
        float width = mapSize.x;
        float height = mapSize.y;

        if ((width / height) > targetAspect)
        {
            float neededHeight = width / targetAspect;
            float padding = (neededHeight - height) / 2f;
            min.y -= padding;
            max.y += padding;
        }
        else
        {
            float neededWidth = height * targetAspect;
            float padding = (neededWidth - width) / 2f;
            min.x -= padding;
            max.x += padding;
        }

        mapSize = max - min;
        // Debug.Log($"[PlayerMapLoader] Adjusted map size to fit table: {mapSize}");

        float baseTileSize = 35f;
        float baseTileSpacing = 1f;

        float originalTileSize = 1f;
        Renderer renderer = terrainTilePrefab.GetComponent<Renderer>();
        if (renderer == null)
            renderer = terrainTilePrefab.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            originalTileSize = renderer.bounds.size.x;
        }
        else
        {
            Debug.LogWarning("[PlayerMapLoader] No Renderer found on prefab. Default tile size used.");
        }

        float baseScaleFactor = baseTileSize / originalTileSize;
        float currentTileSpacing = baseTileSpacing;

        Dictionary<Vector2Int, ObjectData> terrainDict = new();

        foreach (ObjectData obj in terrainObjects)
        {
            Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(obj.position.x), Mathf.RoundToInt(obj.position.z));
            terrainDict[gridPos] = obj;
        }
        // Debug.Log($"[PlayerMapLoader] Terrain dictionary created with {terrainDict.Count} entries.");

        int gridWidth = Mathf.RoundToInt(mapSize.x);
        int gridHeight = Mathf.RoundToInt(mapSize.y);

        // Apply offset for each player's map
        Vector3 origin = mapParent.position -
    new Vector3((gridWidth - 1) * currentTileSpacing / 2f, 0f, (gridHeight - 1) * currentTileSpacing / 2f);

        // Debug.Log($"[PlayerMapLoader] Origin of map for player {playerIndex}: {origin}, grid size: {gridWidth}x{gridHeight}");

        int tilesSpawned = 0;
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector2Int tileGridPos = new Vector2Int(
                    Mathf.FloorToInt(min.x) + x,
                    Mathf.FloorToInt(min.y) + y
                );

                if (terrainDict.TryGetValue(tileGridPos, out ObjectData tileData))
                {
                    Vector3 tilePos = origin + new Vector3(x * currentTileSpacing, 0f, y * currentTileSpacing);
                    GameObject tile = Instantiate(terrainTilePrefab, tilePos, Quaternion.identity, mapParent);
                    tile.transform.localScale = Vector3.one * baseScaleFactor;

                    Renderer tileRenderer = tile.GetComponent<Renderer>();
                    if (tileRenderer == null)
                        tileRenderer = tile.GetComponentInChildren<Renderer>();

                    /*
                    if (tileRenderer != null)
                        tileRenderer.material.color = TerrainTypeToColor(tileData.terrainType);
                    else
                        Debug.LogWarning("[PlayerMapLoader] Renderer missing on spawned tile!");
                    */

                    playerMaps[playerIndex].Add(tile);
                    tilesSpawned++;
                }
            }
        }

        // Debug.Log($"[PlayerMapLoader] Finished building map for player {playerIndex}. Total tiles spawned: {tilesSpawned}");
    }
    
    private void BuildGAEAMapForPlayer(SaveData data, int playerIndex)
    {
        GAEAMapData mapData = data.gaeaMapData;
        
        // Check if files exist
        if (!System.IO.File.Exists(mapData.imagePath) || !System.IO.File.Exists(mapData.objPath))
        {
            Debug.LogError($"[PlayerMapLoader] GAEA files not found for player {playerIndex}");
            return;
        }
        
        // Load texture
        Texture2D texture = LoadTextureFromFile(mapData.imagePath);
        if (texture == null) return;
        
        // Load 3D object
        GameObject mapObject = OBJLoader.LoadOBJFromFile(mapData.objPath);
        if (mapObject == null) return;
        
        // Apply texture
        Renderer renderer = mapObject.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.mainTexture = texture;
            mat.SetFloat("_Glossiness", 0f);
            renderer.material = mat;
            
            // Set up ground layer for player maps
            GameObject meshObj = renderer.gameObject;
            meshObj.layer = 3; // Use same ground layer as GameMaster
            
            MeshCollider collider = meshObj.GetComponent<MeshCollider>();
            if (collider == null)
            {
                collider = meshObj.AddComponent<MeshCollider>();
                MeshFilter meshFilter = meshObj.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    collider.sharedMesh = meshFilter.mesh;
                }
            }
        }
        
        // Position and scale for player view using assigned CameraMover
        mapObject.transform.SetParent(mapAnchors[playerIndex]);
        
        // Use assigned CameraMover for this player
        if (playerIndex < playerCameraMovers.Count && playerCameraMovers[playerIndex] != null)
        {
            CameraMover cameraMover = playerCameraMovers[playerIndex];
            
            // Calculate scale based on CameraMover constraints
            float constraintSizeX = cameraMover.constraintPosX + cameraMover.constraintNegX;
            float constraintSizeZ = cameraMover.constraintPosZ + cameraMover.constraintNegZ;
            float maxConstraint = Mathf.Max(constraintSizeX, constraintSizeZ);
            
            // Scale map to fit within camera viewing area
            float playerScale = maxConstraint * 3.2f; // 4x larger than previous
            
            mapObject.transform.localScale = Vector3.one * playerScale;
            mapObject.transform.localPosition = Vector3.zero;
        }
        else
        {
            // Fallback scaling
            mapObject.transform.localScale = Vector3.one * 40f;
            mapObject.transform.localPosition = Vector3.zero;
        }
        
        mapObject.name = $"GAEAMap_Player{playerIndex + 1}";
        
        // Add to player maps list
        playerMaps[playerIndex].Add(mapObject);
        
        Debug.Log($"[PlayerMapLoader] Built GAEA map for player {playerIndex + 1} with scale: {mapObject.transform.localScale.x}");
    }
    
    private Texture2D LoadTextureFromFile(string path)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

    private Color TerrainTypeToColor(string terrainType)
    {
        return terrainType switch
        {
            "None" => new Color(1f, 1f, 1f, 0f),
            "Grass" => new Color(0.2f, 0.8f, 0.2f),
            "Sand" => new Color(0.9f, 0.8f, 0.5f),
            "Water" => new Color(0.1f, 0.4f, 0.8f),
            "Rock" => new Color(0.6f, 0.6f, 0.6f),
            "Gravel" => new Color(0.5f, 0.5f, 0.5f),
            "DirtRoad" => new Color(0.4f, 0.25f, 0.1f),
            "Hill" => new Color(0.3f, 0.6f, 0.3f),
            "Forest" => new Color(0.0f, 0.4f, 0.0f),
            "Asphalt" => new Color(0.2f, 0.2f, 0.2f),
            "Mud" => new Color(0.3f, 0.2f, 0.1f),
            "Snow" => new Color(0.9f, 0.9f, 1.0f),
            _ => Color.magenta
        };
    }
}