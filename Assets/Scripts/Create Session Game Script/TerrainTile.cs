using UnityEngine;
using System.Collections.Generic;

public class TerrainTile : MonoBehaviour
{
    public enum TerrainType
    {
        None,
        Grass,
        Sand,
        Water,
        Rock,
        Gravel,
        DirtRoad,
        Hill,
        Forest,
        Asphalt,
        Mud,
        Snow
    }

    public TerrainType terrainType;

    private static readonly Dictionary<TerrainType, float> terrainHeights = new Dictionary<TerrainType, float>
    {
        { TerrainType.Water, 1.5f },
        { TerrainType.Sand, 4.0f },
        { TerrainType.Grass, 10.0f },
        { TerrainType.Rock, 22.5f },
        { TerrainType.Gravel, 6.0f },
        { TerrainType.DirtRoad, 5.0f },
        { TerrainType.Hill, 15.0f },
        { TerrainType.Forest, 11.0f },
        { TerrainType.Asphalt, 5.5f },
        { TerrainType.Mud, 4.5f },
        { TerrainType.Snow, 8.0f },
        { TerrainType.None, 1.0f }
    };

    private static readonly Dictionary<TerrainType, Color> terrainColors = new Dictionary<TerrainType, Color>
    {
        { TerrainType.None, new Color(1f, 1f, 1f, 0f) },
        { TerrainType.Grass, new Color(0.2f, 0.8f, 0.2f) },
        { TerrainType.Sand, new Color(0.9f, 0.8f, 0.5f) },
        { TerrainType.Water, new Color(0.1f, 0.4f, 0.8f) },
        { TerrainType.Rock, new Color(0.6f, 0.6f, 0.6f) },
        { TerrainType.Gravel, new Color(0.5f, 0.5f, 0.5f) },
        { TerrainType.DirtRoad, new Color(0.4f, 0.25f, 0.1f) },
        { TerrainType.Hill, new Color(0.3f, 0.6f, 0.3f) },
        { TerrainType.Forest, new Color(0.0f, 0.4f, 0.0f) },
        { TerrainType.Asphalt, new Color(0.2f, 0.2f, 0.2f) },
        { TerrainType.Mud, new Color(0.3f, 0.2f, 0.1f) },
        { TerrainType.Snow, new Color(0.9f, 0.9f, 1.0f) }
    };

    private Renderer tileRenderer;

    void Start()
    {
        tileRenderer = GetComponentInChildren<Renderer>();

        if (tileRenderer == null)
        {
            Debug.LogWarning("No Renderer found in TerrainTile");
        }

        if (terrainColors.TryGetValue(terrainType, out Color color))
        {
            tileRenderer.material.color = color;
        }

        UpdateAppearance();
    }

    public void SetTerrainType(TerrainType newType)
    {
        terrainType = newType;
        UpdateAppearance();
    }

    public void UpdateAppearance()
    {
        if (terrainHeights.TryGetValue(terrainType, out float height))
        {
            transform.localScale = new Vector3(1, height, 1);
            transform.position = new Vector3(transform.position.x, height / 2f, transform.position.z);
        }

        if (tileRenderer != null && terrainColors.TryGetValue(terrainType, out Color color))
        {
            tileRenderer.material.color = color;
        }
    }

    public TerrainType GetTerrainType()
    {
        return terrainType;
    }
}