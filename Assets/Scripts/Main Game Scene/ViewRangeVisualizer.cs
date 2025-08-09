using System.Collections.Generic;
using UnityEngine;

public class ViewRangeVisualizer : MonoBehaviour
{
    public float radius = 12f;
    public float tileSize = 6f;
    public GameObject edgeSegmentPrefab;

    private List<GameObject> ringSegments = new List<GameObject>();

    public void ShowRing()
    {
        ClearRing();

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
    }

    private void TryDrawEdge(int x, int z, int offsetX, int offsetZ)
    {
        float rSquared = radius * radius;

        Vector2 neighborCenter = new Vector2((x + offsetX) * tileSize, (z + offsetZ) * tileSize);
        if (neighborCenter.sqrMagnitude > rSquared)
        {
            // Use local position relative to the preview object (no addition of transform.position)
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

            GameObject edge = Instantiate(edgeSegmentPrefab, transform);
            LineRenderer lr = edge.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            ringSegments.Add(edge);
        }
    }

    public void ClearRing()
    {
        foreach (GameObject segment in ringSegments)
        {
            Destroy(segment);
        }
        ringSegments.Clear();
    }
}