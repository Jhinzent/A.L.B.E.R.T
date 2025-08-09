using System.Collections.Generic;
using UnityEngine;

public class PlayerMapLoader : MonoBehaviour
{
    public GameObject terrainTilePrefab; // Your map tile prefab (2D representation)
    public Transform mapParent;          // Where to place the map (e.g., on a table)
    public Vector2 tableSize = new Vector2(1.75f, 1f); // Table width/height ratio
    public string saveName;
    public GameMasterMapLoader gameMasterMapLoader;

    // Keep track of instantiated tiles so they can be deleted on reload
    private readonly List<GameObject> instantiatedTiles = new();

    void Start()
    {
        saveName = gameMasterMapLoader.getGameSessionSaveName();
        LoadMapFromSave();
    }

    private void LoadMapFromSave()
    {
        SaveData data = SaveSystem.LoadSession(saveName);
        if (data != null)
        {
            BuildMap(data);
        }
        else
        {
            Debug.LogWarning($"No save data found for '{saveName}'");
        }
    }

    // Public method to delete and reload the map
    public void ReloadMap()
    {
        DeleteCurrentMap();
        LoadMapFromSave();
    }

    // Deletes all current tiles from the map
    private void DeleteCurrentMap()
    {
        foreach (GameObject tile in instantiatedTiles)
        {
            if (tile != null)
                Destroy(tile);
        }
        instantiatedTiles.Clear();
    }

    void BuildMap(SaveData data)
    {
        List<ObjectData> terrainObjects = new();

        foreach (ObjectData obj in data.objects)
        {
            if (System.Enum.TryParse(obj.terrainType, out PlaceableItem.TerrainType terrainType))
            {
                terrainObjects.Add(obj);
            }
        }

        if (terrainObjects.Count == 0)
        {
            Debug.LogWarning("No terrain tiles to display.");
            return;
        }

        // Step 1: Calculate map bounds
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (ObjectData obj in terrainObjects)
        {
            Vector2 pos2D = new Vector2(obj.position.x, obj.position.z);
            min = Vector2.Min(min, pos2D);
            max = Vector2.Max(max, pos2D);
        }

        Vector2 mapSize = (max - min) + Vector2.one;

        // Step 2: Adjust to fit aspect ratio
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

        // Step 3: Set base values and prepare for scaling
        float baseTileSize = 35f;  // Desired default tile size before scaling
        float baseTileSpacing = 1f; // Desired default spacing before scaling

        // Get actual size of the tile prefab
        float originalTileSize = 1f;
        Renderer renderer = terrainTilePrefab.GetComponent<Renderer>();
        if (renderer == null)
            renderer = terrainTilePrefab.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            originalTileSize = renderer.bounds.size.x;
        }

        // Calculate the base scale to make tile match 21.7f in world size
        float baseScaleFactor = baseTileSize / originalTileSize;

        float currentTileSpacing = baseTileSpacing;

        // Step 4: Create terrain dictionary
        Dictionary<Vector2Int, ObjectData> terrainDict = new();

        foreach (ObjectData obj in terrainObjects)
        {
            Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(obj.position.x), Mathf.RoundToInt(obj.position.z));
            terrainDict[gridPos] = obj;
        }

        // Step 5: Instantiate tiles
        int gridWidth = Mathf.RoundToInt(mapSize.x);
        int gridHeight = Mathf.RoundToInt(mapSize.y);

        Vector3 origin = mapParent.position - new Vector3((gridWidth - 1) * currentTileSpacing / 2f, 0f, (gridHeight - 1) * currentTileSpacing / 2f);

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

                    // Scale the tile to fit map size
                    tile.transform.localScale = Vector3.one * baseScaleFactor;

                    Renderer tileRenderer = tile.GetComponent<Renderer>();
                    if (tileRenderer == null)
                    {
                        tileRenderer = tile.GetComponentInChildren<Renderer>();
                    }

                    if (tileRenderer != null)
                    {
                        tileRenderer.material.color = TerrainTypeToColor(tileData.terrainType);
                    }
                    else
                    {
                        Debug.LogWarning("Renderer missing on tile prefab!");
                    }

                    instantiatedTiles.Add(tile); // Track instantiated tile
                }
            }
        }
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