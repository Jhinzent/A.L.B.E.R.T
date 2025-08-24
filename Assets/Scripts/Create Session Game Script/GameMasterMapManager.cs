using UnityEngine;
using System.Collections.Generic;

public class GameMasterMapManager : MonoBehaviour
{
    [Header("Tile Settings")]
    public GameObject tilePrefab;
    public float spacingMultiplier = 0.2f; // Multiplier on the prefab’s width to get tile spacing

    // Internal grid data
    private TerrainTile[,] gridTiles;
    private int gridWidth = 800;
    private int gridHeight = 500;
    private float originX, originZ;   // world‐space of grid[0,0]
    private float spacing;            // world‐space distance between grid cells

    // Call this on a fresh session
    public void GenerateGrid()
    {
        Debug.Log($"Generating grid of size: {gridWidth} x {gridHeight}");

        if (tilePrefab == null)
        {
            Debug.LogError("tilePrefab is not assigned in TerrainManager!");
            return;
        }

        // Determine tile size from prefab’s renderer
        float tileSize = 0.2f;
        var rend = tilePrefab.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            tileSize = rend.bounds.size.x;
            Debug.Log($"Detected tileSize: {tileSize}");
        }

        spacing = tileSize * spacingMultiplier;
        originX = -((gridWidth * spacing) / 2f) + spacing / 2f;
        originZ = -((gridHeight * spacing) / 2f) + spacing / 2f;
        Debug.Log($"Origin set to ({originX:F2}, {originZ:F2}), spacing = {spacing:F2}");

        gridTiles = new TerrainTile[gridWidth, gridHeight];

        var parent = new GameObject("TileContainer");

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos = new Vector3(
                    originX + x * spacing,
                    0f,
                    originZ + y * spacing
                );

                var go = Instantiate(tilePrefab, worldPos, Quaternion.identity, parent.transform);
                var tile = go.GetComponent<TerrainTile>();
                if (tile != null)
                {
                    gridTiles[x, y] = tile;
                }
                else
                {
                    Debug.LogError($"Missing TerrainTile on prefab at {worldPos}");
                }
            }
        }

        Debug.Log("Grid generation complete.");
    }

    // Call this after loading saved tiles
    public void RebuildGridFromTiles(List<TerrainTile> tiles)
    {

        if (tiles == null || tiles.Count == 0)
        {
            Debug.LogError("No tiles provided to rebuild grid.");
            return;
        }

        // Find bounds
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        foreach (var tile in tiles)
        {
            var p = tile.transform.position;
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minZ = Mathf.Min(minZ, p.z);
            maxZ = Mathf.Max(maxZ, p.z);
        }

        // Determine spacing using prefab’s renderer
        float tileSize = 0.2f;
        var rend = tilePrefab?.GetComponentInChildren<Renderer>();
        if (rend != null) tileSize = rend.bounds.size.x;
        spacing = tileSize * spacingMultiplier;

        // Compute grid dims
        gridWidth  = Mathf.CeilToInt((maxX - minX) / spacing) + 1;
        gridHeight = Mathf.CeilToInt((maxZ - minZ) / spacing) + 1;
        originX = minX;
        originZ = minZ;

        // Allocate and fill
        gridTiles = new TerrainTile[gridWidth, gridHeight];
        foreach (var tile in tiles)
        {
            var p = tile.transform.position;
            int ix = Mathf.RoundToInt((p.x - originX) / spacing);
            int iy = Mathf.RoundToInt((p.z - originZ) / spacing);

            if (ix >= 0 && ix < gridWidth && iy >= 0 && iy < gridHeight)
            {
                gridTiles[ix, iy] = tile;
            }
            else
            {
                Debug.LogWarning($"Tile “{tile.name}” out of bounds: world {p}, grid ({ix},{iy})");
            }
        }
    }

    // Returns the TerrainTile at worldPos, or null
    public TerrainTile GetTileAtWorldPosition(Vector3 worldPos)
    {
        if (gridTiles == null)
        {
            Debug.LogError("gridTiles is null!");
            return null;
        }

        float localX = worldPos.x - originX;
        float localZ = worldPos.z - originZ;
        int ix = Mathf.RoundToInt(localX / spacing);
        int iy = Mathf.RoundToInt(localZ / spacing);


        if (ix >= 0 && ix < gridWidth && iy >= 0 && iy < gridHeight)
        {
            var tile = gridTiles[ix, iy];
            return tile;
        }
        Debug.LogWarning($"→ Indices out of bounds: ({ix},{iy})");
        return null;
    }

    // Other query helpers (unchanged):
    public TerrainTile GetTileAt(int x, int y)
        => (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight) ? gridTiles[x, y] : null;

    public TerrainTile.TerrainType GetTerrainTypeAt(int x, int y)
        => GetTileAt(x, y)?.GetTerrainType() ?? TerrainTile.TerrainType.None;

    public List<Vector2Int> FindTilesOfType(TerrainTile.TerrainType type)
    {
        var list = new List<Vector2Int>();
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                if (gridTiles[x, y] != null && gridTiles[x, y].GetTerrainType() == type)
                    list.Add(new Vector2Int(x, y));
        return list;
    }
}