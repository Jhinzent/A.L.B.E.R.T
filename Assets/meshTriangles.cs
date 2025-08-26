using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Builds a Mesh containing a single triangle with uvs.
// Create arrays of vertices, uvs and triangles, and copy them into the mesh.

public class meshTriangles : MonoBehaviour
{
    public Color triangleColor = Color.white;
    
    // Use this for initialization
    void Start()
    {
        gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        mesh.Clear();

        // make changes to the Mesh by creating arrays which contain the new values
        mesh.vertices = new Vector3[] {new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0)};
        mesh.uv = new Vector2[] {new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1)};
        mesh.triangles =  new int[] {0, 1, 2};
        
        // Create material with the specified color
        Material material = new Material(Shader.Find("Standard"));
        material.color = triangleColor;
        renderer.material = material;
    }
}