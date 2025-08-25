using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class TerrainPainter : MonoBehaviour
{
    public enum BrushSize { Single = 1, Medium = 2, Large = 3, XLarge = 5 }
    
    public TerrainTile.TerrainType selectedTerrainType = TerrainTile.TerrainType.None; // Default to None
    public Camera gameMasterCamera;
    private TerrainTile currentTile = null;
    private bool isTerrainSelected = false; // Flag to track if a terrain is selected
    public GeneralSessionManager generalGameSessionmanager;
    private BrushSize currentBrushSize = BrushSize.Single;

    void Update()
    {
        // Do nothing if no terrain has been selected or if pointer is over UI element
        if (!isTerrainSelected || EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButton(0)) // Left click
        {
            RaycastHit hit;

            // Cast a ray from the mouse position
            Ray ray = gameMasterCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                TerrainTile tile = hit.collider.GetComponentInParent<TerrainTile>();
                if (tile != null && tile != currentTile)
                {
                    currentTile = tile;
                    PaintTerrain(tile);
                    
                    if (generalGameSessionmanager != null)
                    {
                        generalGameSessionmanager.setReloadFlagTrue();
                    }
                }
                else if (tile == null)
                {
                    // Show what object and components are intercepting the ray
                    GameObject hitObject = hit.collider.gameObject;
                    string components = string.Join(", ", hitObject.GetComponents<Component>().Select(c => c.GetType().Name));
                    // Debug.Log($"Raycast hit '{hitObject.name}' at {hit.point} but it's not a TerrainTile. Components on hit object: {components}");
                }
            }
        }

        // Right-click cancels the terrain painting
        if (Input.GetMouseButtonDown(1)) // Right-click (button 1)
        {
            ResetTerrainSelection();  // Cancel the painting and reset
            // Debug.Log("Terrain painting cancelled.");
        }
    }

    // Method to change the selected terrain type using an integer
    // Method to change the selected terrain type using an integer
    public void SetSelectedTerrain(int terrainIndex)
    {
        selectedTerrainType = (TerrainTile.TerrainType)terrainIndex;
        isTerrainSelected = true; // Mark that terrain is selected

        // If "None" is selected, no painting will occur until it's reset to something else
        if (selectedTerrainType == TerrainTile.TerrainType.None)
        {
            // Debug.Log("Painting mode set to None, waiting for reset.");
        }
    }

    // Method to reset terrain selection, useful if needed
    public void ResetTerrainSelection()
    {
        selectedTerrainType = TerrainTile.TerrainType.None;
        isTerrainSelected = false; // Reset the flag
        // Debug.Log("Terrain selection reset.");
    }

    public void SetBrushSize1x1() => currentBrushSize = BrushSize.Single;
    public void SetBrushSize2x2() => currentBrushSize = BrushSize.Medium;
    public void SetBrushSize3x3() => currentBrushSize = BrushSize.Large;
    public void SetBrushSize5x5() => currentBrushSize = BrushSize.XLarge;

    private void PaintTerrain(TerrainTile centerTile)
    {
        int size = (int)currentBrushSize;
        Vector3 centerPos = centerTile.transform.position;
        float tileSize = GetTileSize(centerTile);
        
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                float offsetX, offsetZ;
                if (size == 2)
                {
                    offsetX = x * tileSize;
                    offsetZ = z * tileSize;
                }
                else
                {
                    offsetX = (x - size/2) * tileSize;
                    offsetZ = (z - size/2) * tileSize;
                }
                
                Vector3 targetPos = centerPos + new Vector3(offsetX, 0, offsetZ);
                
                Ray ray = new Ray(targetPos + Vector3.up * 10, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit, 20f))
                {
                    TerrainTile tile = hit.collider.GetComponentInParent<TerrainTile>();
                    if (tile != null)
                    {
                        tile.SetTerrainType(selectedTerrainType);
                    }
                }
            }
        }
    }
    
    private float GetTileSize(TerrainTile tile)
    {
        Vector3 tilePos = tile.transform.position;
        float[] testDistances = { 1f, 2f, 3f, 4f, 5f, 10f };
        
        foreach (float testDist in testDistances)
        {
            Vector3 testPos = tilePos + Vector3.right * testDist;
            Ray ray = new Ray(testPos + Vector3.up * 10, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 20f))
            {
                TerrainTile nearbyTile = hit.collider.GetComponentInParent<TerrainTile>();
                if (nearbyTile != null && nearbyTile != tile)
                {
                    return Vector3.Distance(tilePos, nearbyTile.transform.position);
                }
            }
        }
        
        Collider col = tile.GetComponent<Collider>();
        return col != null ? col.bounds.size.x : 1f;
    }
}