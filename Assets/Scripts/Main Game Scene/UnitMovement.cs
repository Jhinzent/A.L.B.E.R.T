using UnityEngine;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(LineRenderer))]
public class UnitMovement : MonoBehaviour
{
    [Header("References")]
    public LineRenderer pathRenderer;
    public GameMasterMapManager terrainManager;      // auto-assigned if null
    public TextMeshProUGUI costText;           // Assign in Inspector

    [Header("Path Settings")]
    public int bezierSegments = 20;
    public float controlPointHeight = 200f;

    private List<Vector3> currentPath = new List<Vector3>();
    private bool isInMovementMode = false;
    private Vector3 startPosition;
    private Quaternion initialCostTextRotation;
    private Vector3 initialCostTextPosition;

    void Awake()
    {
        if (terrainManager == null)
            terrainManager = FindObjectOfType<GameMasterMapManager>();
            
        if (costText != null)
        {
            initialCostTextRotation = costText.transform.rotation;
            initialCostTextPosition = costText.transform.position;
        }

        pathRenderer = GetComponent<LineRenderer>()
                   ?? gameObject.AddComponent<LineRenderer>();
        pathRenderer.positionCount = 0;
        pathRenderer.widthMultiplier = 2.5f;
        pathRenderer.material = new Material(Shader.Find("Unlit/Color"));
        pathRenderer.material.color = Color.red;
        pathRenderer.enabled = false;

        if (costText != null)
            costText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (costText != null && costText.gameObject.activeInHierarchy)
        {
            costText.transform.rotation = initialCostTextRotation;
            costText.transform.position = initialCostTextPosition;
        }
        
        if (!isInMovementMode)
            return;

        Vector3 mouseWorld = GetMouseWorldPosition();
        if (mouseWorld == Vector3.zero)
            return;

        // 1. Generate points
        Vector3[] points = GenerateCurvedPath(startPosition, mouseWorld);

        // 2. Draw line
        pathRenderer.enabled = true;
        pathRenderer.positionCount = points.Length;
        pathRenderer.SetPositions(points);

        // 3. Compute & update cost
        float cost = ComputePathCost(points);
        if (costText != null)
        {
            int displayCost = Mathf.RoundToInt(cost / 10f) * 10;
            costText.text = $"Cost: {displayCost}";
        }

        // 4. Confirm on click
        if (Input.GetMouseButtonDown(0))
        {
            currentPath = new List<Vector3>(points);
            ExitMovementMode();
        }
    }

    public void EnterMovementMode()
    {
        startPosition = transform.position;
        currentPath.Clear();
        isInMovementMode = true;
        pathRenderer.enabled = true;

        // Enable cost text when entering movement mode
        if (costText != null)
            costText.gameObject.SetActive(true);
    }

    public void ExitMovementMode()
    {
        isInMovementMode = false;
        pathRenderer.positionCount = 0;
        DisplaySavedPath();

        // Disable cost text when exiting movement mode
        if (costText != null)
            costText.gameObject.SetActive(false);
    }

    private float ComputePathCost(Vector3[] points)
    {
        if (terrainManager == null || points == null || points.Length < 2)
            return 0f;

        // Costs per terrain
        var costMap = new Dictionary<TerrainTile.TerrainType, float>
        {
            { TerrainTile.TerrainType.None,     1f   },
            { TerrainTile.TerrainType.Grass,    1f   },
            { TerrainTile.TerrainType.Sand,     1.5f },
            { TerrainTile.TerrainType.Water,    3f   },
            { TerrainTile.TerrainType.Rock,     4f   },
            { TerrainTile.TerrainType.Gravel,   1.2f },
            { TerrainTile.TerrainType.DirtRoad, 0.5f },
            { TerrainTile.TerrainType.Hill,     2f   },
            { TerrainTile.TerrainType.Forest,   2.5f },
            { TerrainTile.TerrainType.Asphalt,  0.8f },
            { TerrainTile.TerrainType.Mud,      2f   },
            { TerrainTile.TerrainType.Snow,     1.8f }
        };

        float total = 0f;
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 segmentStart = points[i];
            Vector3 segmentEnd = points[i + 1];
            float segmentDistance = Vector3.Distance(segmentStart, segmentEnd);
            
            // Use terrain at segment midpoint
            Vector3 midPoint = (segmentStart + segmentEnd) * 0.5f;
            var tile = terrainManager.GetTileAtWorldPosition(midPoint);
            
            TerrainTile.TerrainType terrainType = tile?.GetTerrainType() ?? TerrainTile.TerrainType.None;
            float terrainCost = costMap.ContainsKey(terrainType) ? costMap[terrainType] : 1f;
            
            total += segmentDistance * terrainCost;
        }
        
        return total;
    }

    public void HidePath()
    {
        if (pathRenderer != null)
        {
            pathRenderer.enabled = false;
        }
    }


    public void DisplaySavedPath()
    {
        if (currentPath.Count > 0)
        {
            pathRenderer.positionCount = currentPath.Count;
            pathRenderer.SetPositions(currentPath.ToArray());
            pathRenderer.enabled = true;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit) ? hit.point : Vector3.zero;
    }

    private Vector3[] GenerateCurvedPath(Vector3 a, Vector3 b)
    {
        var pts = new Vector3[bezierSegments + 1];
        Vector3 c = (a + b) * 0.5f + Vector3.up * controlPointHeight;
        for (int i = 0; i <= bezierSegments; i++)
        {
            float t = i / (float)bezierSegments;
            pts[i] = (1 - t) * (1 - t) * a + 2 * (1 - t) * t * c + t * t * b;
        }
        return pts;
    }
}