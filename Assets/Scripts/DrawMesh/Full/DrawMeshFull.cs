using System.Collections.Generic;
using UnityEngine;

public class DrawMeshFull : MonoBehaviour
{
    public static DrawMeshFull Instance { get; private set; }

    [Header("Drawing Settings")]
    public Material drawMaterial;
    public float thickness = 0.5f;
    public float minDistance = 0.05f;
    public float heightOffset = 0.01f;
    public int smoothingSteps = 3;
    public Texture2D drawCursor; // Assign a small icon texture in Inspector
    
    [Header("Treasure Map Mode")]
    public float gapInterval = 0.1f;
    public float gapLength = 0.05f;
    
    private List<GameObject> drawnLines = new List<GameObject>();
    private GameObject currentDrawGO;
    private Mesh mesh;
    private List<Vector3> currentLinePoints = new List<Vector3>();
    private Color color = Color.red;
    private bool treasureMapMode = false;
    private float gapCounter = 0f;

    public bool isDrawing = false;
    public bool drawingEnabled = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && drawingEnabled)
        {
            // Don't start drawing if clicking over UI
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                StartNewLine();
            }
        }

        if (Input.GetMouseButton(0) && isDrawing)
        {
            Vector3 mousePos = GetMouseWorldPosition();
            if (currentLinePoints.Count == 0 || Vector3.Distance(mousePos, currentLinePoints[currentLinePoints.Count - 1]) > minDistance)
            {
                currentLinePoints.Add(mousePos);
                UpdateCurrentLine();
            }
        }

        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            FinishCurrentLine();
        }

        if (Input.GetMouseButtonDown(1))
        {
            drawingEnabled = false;
            // Reset cursor to default
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    public void ActivateDrawing(Color col)
    {
        color = col;
        drawingEnabled = true;
        
        // Set custom cursor
        if (drawCursor != null && drawCursor.isReadable)
        {
            Cursor.SetCursor(drawCursor, Vector2.zero, CursorMode.Auto);
        }
    }

    public void ClearAllDrawings()
    {
        foreach (GameObject line in drawnLines)
        {
            Destroy(line);
        }
        drawnLines.Clear();
    }

    private void StartNewLine()
    {
        currentDrawGO = new GameObject("LineDraw", typeof(MeshFilter), typeof(MeshRenderer));
        currentDrawGO.GetComponent<MeshRenderer>().material = new Material(drawMaterial) { color = color };
        
        mesh = new Mesh();
        currentDrawGO.GetComponent<MeshFilter>().mesh = mesh;
        
        drawnLines.Add(currentDrawGO);
        currentLinePoints.Clear();
        
        Vector3 startPos = GetMouseWorldPosition();
        currentLinePoints.Add(startPos);
        
        isDrawing = true;
        gapCounter = 0f;
    }
    
    private void UpdateCurrentLine()
    {
        if (currentLinePoints.Count < 2) return;
        
        List<Vector3> smoothedPoints = SmoothLine(currentLinePoints);
        RebuildMesh(smoothedPoints);
    }
    
    private void FinishCurrentLine()
    {
        if (currentLinePoints.Count >= 2)
        {
            List<Vector3> smoothedPoints = SmoothLine(currentLinePoints);
            RebuildMesh(smoothedPoints);
        }
        
        isDrawing = false;
        currentLinePoints.Clear();
    }
    
    private List<Vector3> SmoothLine(List<Vector3> points)
    {
        if (points.Count < 3) return points;
        
        List<Vector3> smoothed = new List<Vector3>(points);
        
        for (int step = 0; step < smoothingSteps; step++)
        {
            List<Vector3> newSmoothed = new List<Vector3> { smoothed[0] };
            
            for (int i = 1; i < smoothed.Count - 1; i++)
            {
                Vector3 smoothPoint = (smoothed[i - 1] + smoothed[i] * 2f + smoothed[i + 1]) * 0.25f;
                newSmoothed.Add(smoothPoint);
            }
            
            newSmoothed.Add(smoothed[smoothed.Count - 1]);
            smoothed = newSmoothed;
        }
        
        return smoothed;
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << 3))
        {
            Vector3 point = hit.point;
            point.y += heightOffset;
            return point;
        }
        
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        
        return Vector3.zero;
    }

    public void SetThickness(float value) => thickness = value;

    public void SetNormalDrawingMode()
    {
        treasureMapMode = false;
    }

    public void SetTreasureMapMode()
    {
        treasureMapMode = true;
        gapCounter = 0f;
    }

    private void RebuildMesh(List<Vector3> points)
    {
        if (points.Count < 2) return;
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();
        
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (treasureMapMode && !ShouldDrawSegment(i)) continue;
            
            Vector3 current = points[i];
            Vector3 next = points[i + 1];
            
            Vector3 forward = (next - current).normalized;
            Vector3 right = Vector3.Cross(forward, Vector3.up).normalized * thickness * 0.5f;
            
            int baseIndex = vertices.Count;
            
            vertices.Add(current - right);
            vertices.Add(current + right);
            vertices.Add(next + right);
            vertices.Add(next - right);
            
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(1, 1));
            uv.Add(new Vector2(0, 1));
            
            triangles.Add(baseIndex + 0);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 0);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 3);
        }
        
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
    
    private bool ShouldDrawSegment(int segmentIndex)
    {
        float position = segmentIndex * minDistance;
        float cyclePosition = position % (gapInterval + gapLength);
        return cyclePosition < gapInterval;
    }
}