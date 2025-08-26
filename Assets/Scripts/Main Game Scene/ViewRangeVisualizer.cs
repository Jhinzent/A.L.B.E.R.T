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
            lr.SetPosition(0, transform.position + start);
            lr.SetPosition(1, transform.position + end);
            
            // Ensure LineRenderer is visible
            lr.enabled = true;
            
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
    
    private void OnDestroy()
    {
        // Debug.Log($"[ViewRangeVisualizer] OnDestroy() called on {gameObject.name} (ID:{GetInstanceID()})");
        ClearRing();
    }
}
