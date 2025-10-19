using System.Collections.Generic;
using UnityEngine;

public class ViewRangeVisualizer : MonoBehaviour
{
    public float radius = 12f;
    public float tileSize = 1f;
    public GameObject edgeSegmentPrefab;

    private List<GameObject> ringSegments = new List<GameObject>();
    private bool isRingVisible = false;
    private Vector3 lastPosition;
    
    private void Update()
    {
        if (isRingVisible && transform.position != lastPosition)
        {
            UpdateRingPosition();
            lastPosition = transform.position;
        }
    }

    public void ShowRing()
    {
        // Debug.Log($"[ViewRangeVisualizer] ShowRing() START on {gameObject.name} (ID:{GetInstanceID()}) at position {transform.position}");
        if (edgeSegmentPrefab == null)
        {
            Debug.LogError("EdgeSegmentPrefab is null on " + gameObject.name);
            return;
        }

        ClearRing();
        isRingVisible = true;
        lastPosition = transform.position;

        float rSquared = radius * radius;
        int gridRadius = Mathf.CeilToInt(radius / tileSize);

        for (int x = -gridRadius; x <= gridRadius; x++)
        {
            for (int z = -gridRadius; z <= gridRadius; z++)
            {
                Vector2 tileCenter = new Vector2(x * tileSize, z * tileSize);
                if (tileCenter.sqrMagnitude <= rSquared)
                {
                    TryDrawEdge(x, z, 1, 0);    // Right
                    TryDrawEdge(x, z, -1, 0);   // Left
                    TryDrawEdge(x, z, 0, 1);    // Forward
                    TryDrawEdge(x, z, 0, -1);   // Backward
                }
            }
        }
        
        // Debug.Log($"[ViewRangeVisualizer] ShowRing() END on {gameObject.name} (ID:{GetInstanceID()}) - Created {ringSegments.Count} segments");
        
        // Check if segments are still valid after creation
        StartCoroutine(CheckSegmentsAfterFrame());
    }

    private void TryDrawEdge(int x, int z, int offsetX, int offsetZ)
    {
        float rSquared = radius * radius;

        Vector2 neighborCenter = new Vector2((x + offsetX) * tileSize, (z + offsetZ) * tileSize);
        if (neighborCenter.sqrMagnitude > rSquared)
        {
            Vector3 tilePos = new Vector3(x * tileSize, 0f, z * tileSize);

            Vector3 start, end;

            if (offsetX != 0)
            {
                start = tilePos + new Vector3(offsetX * tileSize / 2f, 0f, -tileSize / 2f);
                end = tilePos + new Vector3(offsetX * tileSize / 2f, 0f, tileSize / 2f);
            }
            else // offsetZ != 0
            {
                start = tilePos + new Vector3(-tileSize / 2f, 0f, offsetZ * tileSize / 2f);
                end = tilePos + new Vector3(tileSize / 2f, 0f, offsetZ * tileSize / 2f);
            }

            GameObject edge = Instantiate(edgeSegmentPrefab);
            edge.transform.SetParent(null); // Keep in world space
            edge.name = $"ViewRange_{gameObject.GetInstanceID()}_{System.Guid.NewGuid().ToString().Substring(0,8)}_{ringSegments.Count}";
            
            LineRenderer lr = edge.GetComponent<LineRenderer>();
            if (lr == null)
            {
                // Debug.LogError($"[ViewRangeVisualizer] No LineRenderer on edgeSegmentPrefab!");
                Destroy(edge);
                return;
            }
            
            lr.positionCount = 2;
            
            // Get terrain height at start and end positions
            Vector3 worldStart = transform.position + start;
            Vector3 worldEnd = transform.position + end;
            
            float startHeight = GetTerrainHeight(worldStart);
            float endHeight = GetTerrainHeight(worldEnd);
            
            // Position lines above terrain surface
            Vector3 elevatedStart = new Vector3(worldStart.x, startHeight + 1f, worldStart.z);
            Vector3 elevatedEnd = new Vector3(worldEnd.x, endHeight + 1f, worldEnd.z);
            
            lr.SetPosition(0, elevatedStart);
            lr.SetPosition(1, elevatedEnd);
            
            // Ensure LineRenderer renders on top
            lr.enabled = true;
            lr.sortingOrder = 100; // High sorting order
            
            // Set material to render on top of terrain
            if (lr.material != null)
            {
                lr.material.renderQueue = 3000; // Transparent queue
            }
            
            // Only log first segment to avoid spam
            if (ringSegments.Count == 0)
            {
                // Debug.Log($"[ViewRangeVisualizer] First segment: {edge.name} at {edge.transform.position}, LR: {lr.GetPosition(0)} to {lr.GetPosition(1)}");
            }

            ringSegments.Add(edge);
        }
    }

    public void ClearRing()
    {
        // Debug.Log($"[ViewRangeVisualizer] ClearRing() called on {gameObject.name} (ID:{GetInstanceID()}) - Clearing {ringSegments.Count} segments");
        foreach (GameObject segment in ringSegments)
        {
            if (segment != null)
                Destroy(segment);
        }
        ringSegments.Clear();
        isRingVisible = false;
        lastPosition = Vector3.zero;
    }
    
    public bool IsRingVisible()
    {
        return isRingVisible;
    }
    
    public void UpdateRingPosition()
    {
        Vector3 positionDelta = transform.position - lastPosition;
        
        foreach (GameObject segment in ringSegments)
        {
            if (segment != null)
            {
                LineRenderer lr = segment.GetComponent<LineRenderer>();
                if (lr != null && lr.positionCount == 2)
                {
                    Vector3 pos0 = lr.GetPosition(0) + positionDelta;
                    Vector3 pos1 = lr.GetPosition(1) + positionDelta;
                    
                    // Update height based on new terrain position
                    pos0.y = GetTerrainHeight(pos0) + 1f;
                    pos1.y = GetTerrainHeight(pos1) + 1f;
                    
                    lr.SetPosition(0, pos0);
                    lr.SetPosition(1, pos1);
                }
            }
        }
    }
    
    private System.Collections.IEnumerator CheckSegmentsAfterFrame()
    {
        yield return new WaitForSeconds(0.1f); // Wait a bit longer
        int validSegments = 0;
        foreach (GameObject segment in ringSegments)
        {
            if (segment != null) validSegments++;
        }
        // Debug.Log($"[ViewRangeVisualizer] After 0.1s: {validSegments}/{ringSegments.Count} segments valid on {gameObject.name}");
        
        if (validSegments == 0 && ringSegments.Count > 0)
        {
            Debug.LogWarning($"[ViewRangeVisualizer] ALL SEGMENTS DESTROYED on {gameObject.name}!");
        }
    }
    
    private float GetTerrainHeight(Vector3 worldPosition)
    {
        // Raycast down from high above to find terrain
        RaycastHit hit;
        Vector3 rayStart = new Vector3(worldPosition.x, 1000f, worldPosition.z);
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity, 1 << 3)) // Layer 3 = ground
        {
            return hit.point.y;
        }
        
        // Fallback to unit's Y position if no terrain found
        return transform.position.y;
    }
    
    private void OnDestroy()
    {
        // Debug.Log($"[ViewRangeVisualizer] OnDestroy() called on {gameObject.name} (ID:{GetInstanceID()})");
        ClearRing();
    }
}
