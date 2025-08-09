using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TerrainList : MonoBehaviour
{
    public List<TerrainTile.TerrainType> terrainTypes = new List<TerrainTile.TerrainType>();
    public GameObject scrollViewItemPrefab; // Assign the TerrainTemplate prefab
    public Transform contentParent; // Assign the Content object in the Scroll View
    public TerrainPainter terrainPainter;

    private Dictionary<TerrainTile.TerrainType, Color> terrainColors = new Dictionary<TerrainTile.TerrainType, Color>
{
    { TerrainTile.TerrainType.None, Color.gray },
    { TerrainTile.TerrainType.Grass, new Color(0.2f, 0.6f, 0.2f) },
    { TerrainTile.TerrainType.Sand, new Color(0.9f, 0.85f, 0.5f) },
    { TerrainTile.TerrainType.Water, new Color(0.1f, 0.3f, 0.9f) },
    { TerrainTile.TerrainType.Rock, new Color(0.5f, 0.5f, 0.5f) },
    { TerrainTile.TerrainType.Gravel, new Color(0.6f, 0.6f, 0.6f) },
    { TerrainTile.TerrainType.DirtRoad, new Color(0.5f, 0.3f, 0.1f) },
    { TerrainTile.TerrainType.Hill, new Color(0.3f, 0.5f, 0.2f) },
    { TerrainTile.TerrainType.Forest, new Color(0.0f, 0.4f, 0.0f) },
    { TerrainTile.TerrainType.Asphalt, new Color(0.2f, 0.2f, 0.2f) },
    { TerrainTile.TerrainType.Mud, new Color(0.4f, 0.25f, 0.1f) },
    { TerrainTile.TerrainType.Snow, new Color(0.9f, 0.9f, 1.0f) }
};

    void Start()
{
    if (terrainTypes == null || terrainTypes.Count == 0)
    {
        terrainTypes = new List<TerrainTile.TerrainType>
        {
            TerrainTile.TerrainType.Grass,
            TerrainTile.TerrainType.Sand,
            TerrainTile.TerrainType.Water,
            TerrainTile.TerrainType.Rock,
            TerrainTile.TerrainType.Gravel,
            TerrainTile.TerrainType.DirtRoad,
            TerrainTile.TerrainType.Hill,
            TerrainTile.TerrainType.Forest,
            TerrainTile.TerrainType.Asphalt,
            TerrainTile.TerrainType.Mud,
            TerrainTile.TerrainType.Snow
        };
    }

    PopulateScrollView();
}

    void PopulateScrollView()
{
    if (terrainTypes == null || terrainTypes.Count == 0)
    {
        Debug.LogError("TerrainList is null/empty!");
        return;
    }

    foreach (var terrain in terrainTypes)
    {

        GameObject instantiatedItem = Instantiate(scrollViewItemPrefab, contentParent);
        TerrainInScrollView newScrollViewItem = instantiatedItem.GetComponent<TerrainInScrollView>();

        newScrollViewItem.SetTextComponent(terrain.ToString());

        if (terrainColors.TryGetValue(terrain, out Color color))
        {
            newScrollViewItem.SetColorComponent(color);
        }
        else
        {
            Debug.LogError($"Color not found for terrain type: {terrain}");
        }

        // Add button listener for selecting terrain
        Button scrollViewItemButton = newScrollViewItem.GetButtonComponent();
        scrollViewItemButton.onClick.AddListener(() => terrainPainter.SetSelectedTerrain((int)terrain));
    }
}
}