using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class TerrainPainter : MonoBehaviour
{
    public TerrainTile.TerrainType selectedTerrainType = TerrainTile.TerrainType.None; // Default to None
    public Camera gameMasterCamera;
    private TerrainTile currentTile = null;
    private bool isTerrainSelected = false; // Flag to track if a terrain is selected
    public GnerealSessionManager generalGameSessionmanager;

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
                    tile.SetTerrainType(selectedTerrainType);
                }
                else if (tile == null)
                {
                    // Show what object and components are intercepting the ray
                    GameObject hitObject = hit.collider.gameObject;
                    string components = string.Join(", ", hitObject.GetComponents<Component>().Select(c => c.GetType().Name));
                    Debug.Log($"Raycast hit '{hitObject.name}' at {hit.point} but it's not a TerrainTile. Components on hit object: {components}");
                }
            }
        }

        // Right-click cancels the terrain painting
        if (Input.GetMouseButtonDown(1)) // Right-click (button 1)
        {
            ResetTerrainSelection();  // Cancel the painting and reset
            Debug.Log("Terrain painting cancelled.");
        }
    }

    // Method to change the selected terrain type using an integer
    // Method to change the selected terrain type using an integer
    public void SetSelectedTerrain(int terrainIndex)
    {
        if (generalGameSessionmanager != null)
        {
            generalGameSessionmanager.setReloadFlagTrue();
        }

        selectedTerrainType = (TerrainTile.TerrainType)terrainIndex;
        isTerrainSelected = true; // Mark that terrain is selected

        // If "None" is selected, no painting will occur until it's reset to something else
        if (selectedTerrainType == TerrainTile.TerrainType.None)
        {
            Debug.Log("Painting mode set to None, waiting for reset.");
        }
    }

    // Method to reset terrain selection, useful if needed
    public void ResetTerrainSelection()
    {
        selectedTerrainType = TerrainTile.TerrainType.None;
        isTerrainSelected = false; // Reset the flag
        Debug.Log("Terrain selection reset.");
    }
}