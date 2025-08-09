using System.Collections.Generic;
using UnityEngine;

public class DrawMeshFull : MonoBehaviour
{
    public static DrawMeshFull Instance { get; private set; }

    [SerializeField] private Material drawMaterial;

    private List<GameObject> drawnLines = new List<GameObject>();
    private GameObject currentDrawGO;
    private Mesh mesh;
    private Vector3 lastPos;
    private float thickness = 1f;
    private Color color = Color.red;

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
            currentDrawGO = new GameObject("LineDraw", typeof(MeshFilter), typeof(MeshRenderer));
            currentDrawGO.GetComponent<MeshRenderer>().material = new Material(drawMaterial) { color = color };

            mesh = new Mesh();
            currentDrawGO.GetComponent<MeshFilter>().mesh = mesh;

            drawnLines.Add(currentDrawGO);

            lastPos = GetMouseWorldPosition();
            isDrawing = true;
        }

        if (Input.GetMouseButton(0) && isDrawing)
        {
            Vector3 mousePos = GetMouseWorldPosition();
            if (Vector3.Distance(mousePos, lastPos) > 0.05f)
            {
                AddLineSegment(mesh, lastPos, mousePos, thickness);
                lastPos = mousePos;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }

        if (Input.GetMouseButtonDown(1))
        {
            drawingEnabled = false;
        }
    }

    public void ActivateDrawing(Color col)
    {
        color = col;
        drawingEnabled = true;
    }

    public void ClearAllDrawings()
    {
        foreach (GameObject line in drawnLines)
        {
            Destroy(line);
        }
        drawnLines.Clear();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);
            point.y += 0.005f; // 👈 Lift drawing slightly above terrain
            return point;
        }
        return Vector3.zero;
    }

    public void SetThickness(float value) => thickness = value;

    private void AddLineSegment(Mesh mesh, Vector3 lastPos, Vector3 currentPos, float thickness)
    {
        List<Vector3> vertices = new List<Vector3>(mesh.vertices);
        List<int> triangles = new List<int>(mesh.triangles);
        List<Vector2> uv = new List<Vector2>(mesh.uv);

        Vector3 forward = (currentPos - lastPos).normalized;
        Vector3 right = Vector3.Cross(forward, Vector3.up) * thickness;

        Vector3 v1 = lastPos - right;
        Vector3 v2 = lastPos + right;
        Vector3 v3 = currentPos + right;
        Vector3 v4 = currentPos - right;

        int baseIndex = vertices.Count;

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);

        uv.Add(Vector2.zero);
        uv.Add(Vector2.zero);
        uv.Add(Vector2.zero);
        uv.Add(Vector2.zero);

        triangles.Add(baseIndex + 0);
        triangles.Add(baseIndex + 1);
        triangles.Add(baseIndex + 2);
        triangles.Add(baseIndex + 0);
        triangles.Add(baseIndex + 2);
        triangles.Add(baseIndex + 3);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.RecalculateNormals();
    }
}