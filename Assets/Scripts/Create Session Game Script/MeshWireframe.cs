using UnityEngine;

public class MeshWireframe : MonoBehaviour
{
    void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            mat.SetFloat("_Wireframe", 1f);
        }
    }
    
    void OnRenderObject()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.mesh == null) return;
        
        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        
        Material lineMat = new Material(Shader.Find("Hidden/Internal-Colored"));
        lineMat.SetPass(0);
        
        GL.Begin(GL.LINES);
        GL.Color(Color.green);
        
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // Draw triangle edges
            GL.Vertex3(vertices[triangles[i]].x, vertices[triangles[i]].y, vertices[triangles[i]].z);
            GL.Vertex3(vertices[triangles[i + 1]].x, vertices[triangles[i + 1]].y, vertices[triangles[i + 1]].z);
            
            GL.Vertex3(vertices[triangles[i + 1]].x, vertices[triangles[i + 1]].y, vertices[triangles[i + 1]].z);
            GL.Vertex3(vertices[triangles[i + 2]].x, vertices[triangles[i + 2]].y, vertices[triangles[i + 2]].z);
            
            GL.Vertex3(vertices[triangles[i + 2]].x, vertices[triangles[i + 2]].y, vertices[triangles[i + 2]].z);
            GL.Vertex3(vertices[triangles[i]].x, vertices[triangles[i]].y, vertices[triangles[i]].z);
        }
        
        GL.End();
        GL.PopMatrix();
    }
}