using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class OBJLoader
{
    public static GameObject LoadOBJFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("OBJ file not found: " + filePath);
            return null;
        }

        List<Vector3> objVertices = new List<Vector3>();
        List<Vector2> objUVs = new List<Vector2>();
        List<Vector3> objNormals = new List<Vector3>();
        
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();

        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;
                
            string[] parts = trimmedLine.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length == 0) continue;

            switch (parts[0])
            {
                case "v":
                    if (parts.Length >= 4)
                    {
                        if (float.TryParse(parts[1], out float x) && 
                            float.TryParse(parts[2], out float y) && 
                            float.TryParse(parts[3], out float z))
                        {
                            // Unity's OBJ import: X stays, Y stays, Z flips, then scale by 0.01
                            objVertices.Add(new Vector3(x, y, -z) * 0.01f);
                        }
                    }
                    break;

                case "vt":
                    if (parts.Length >= 3)
                    {
                        if (float.TryParse(parts[1], out float u) && 
                            float.TryParse(parts[2], out float v))
                        {
                            objUVs.Add(new Vector2(u, 1f - v)); // Flip V coordinate for Unity
                        }
                    }
                    break;

                case "vn":
                    if (parts.Length >= 4)
                    {
                        if (float.TryParse(parts[1], out float nx) && 
                            float.TryParse(parts[2], out float ny) && 
                            float.TryParse(parts[3], out float nz))
                        {
                            // Flip normal Z to match coordinate system flip
                            objNormals.Add(new Vector3(nx, ny, -nz));
                        }
                    }
                    break;

                case "f":
                    if (parts.Length >= 4)
                    {
                        List<int> vertexIndices = new List<int>();
                        List<int> uvIndices = new List<int>();
                        List<int> normalIndices = new List<int>();
                        
                        for (int i = 1; i < parts.Length; i++)
                        {
                            string[] indices = parts[i].Split('/');
                            
                            // Parse vertex index (required)
                            if (indices.Length > 0 && int.TryParse(indices[0], out int vIndex))
                            {
                                vertexIndices.Add(vIndex - 1);
                            }
                            
                            // Parse UV index (optional)
                            if (indices.Length > 1 && !string.IsNullOrEmpty(indices[1]) && int.TryParse(indices[1], out int uvIndex))
                            {
                                uvIndices.Add(uvIndex - 1);
                            }
                            
                            // Parse normal index (optional)
                            if (indices.Length > 2 && !string.IsNullOrEmpty(indices[2]) && int.TryParse(indices[2], out int nIndex))
                            {
                                normalIndices.Add(nIndex - 1);
                            }
                        }
                        
                        // Build unified vertex/UV/normal arrays
                        if (vertexIndices.Count == 4)
                        {
                            // Quad - create 2 triangles with proper UV mapping
                            int[] quadOrder = {0, 2, 1, 0, 3, 2}; // Reversed winding
                            
                            for (int i = 0; i < 6; i++)
                            {
                                int idx = quadOrder[i];
                                vertices.Add(objVertices[vertexIndices[idx]]);
                                
                                if (uvIndices.Count > idx)
                                    uvs.Add(objUVs[uvIndices[idx]]);
                                else
                                    uvs.Add(Vector2.zero);
                                    
                                if (normalIndices.Count > idx)
                                    normals.Add(objNormals[normalIndices[idx]]);
                                else
                                    normals.Add(Vector3.up);
                                    
                                triangles.Add(vertices.Count - 1);
                            }
                        }
                        else if (vertexIndices.Count == 3)
                        {
                            // Triangle - reversed winding
                            int[] triOrder = {0, 2, 1};
                            
                            for (int i = 0; i < 3; i++)
                            {
                                int idx = triOrder[i];
                                vertices.Add(objVertices[vertexIndices[idx]]);
                                
                                if (uvIndices.Count > idx)
                                    uvs.Add(objUVs[uvIndices[idx]]);
                                else
                                    uvs.Add(Vector2.zero);
                                    
                                if (normalIndices.Count > idx)
                                    normals.Add(objNormals[normalIndices[idx]]);
                                else
                                    normals.Add(Vector3.up);
                                    
                                triangles.Add(vertices.Count - 1);
                            }
                        }
                    }
                    break;
            }
        }

        // Create parent object
        GameObject parentObj = new GameObject(Path.GetFileNameWithoutExtension(filePath));
        
        // Create child object with mesh (like Unity's OBJ import)
        GameObject meshObj = new GameObject("default");
        meshObj.transform.SetParent(parentObj.transform);
        meshObj.layer = 3; // Set to ground layer for ObjectPlacer
        
        MeshFilter meshFilter = meshObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshObj.AddComponent<MeshRenderer>();
        
        // Add collider for ObjectPlacer raycast detection
        MeshCollider meshCollider = meshObj.AddComponent<MeshCollider>();

        Mesh mesh = new Mesh();
        mesh.name = Path.GetFileNameWithoutExtension(filePath) + "_Mesh";
        
        if (vertices.Count == 0)
        {
            Debug.LogError("No vertices found in OBJ file");
            return null;
        }
        
        // Scaling already applied during vertex parsing to match Unity's import
        
        mesh.vertices = vertices.ToArray();
        
        // Ensure triangles array is valid
        if (triangles.Count % 3 == 0 && triangles.Count > 0)
        {
            mesh.triangles = triangles.ToArray();
        }
        else
        {
            Debug.LogError($"Invalid triangle count: {triangles.Count}. Must be multiple of 3.");
            return null;
        }
        
        // Apply UV coordinates and normals (now properly aligned)
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();
        Debug.Log($"UV mapping applied: {uvs.Count} UV coordinates for {vertices.Count} vertices");
        // Don't recalculate normals to preserve sharp terrain edges
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh; // Assign mesh to collider for raycasting
        
        // Use standard material matching Unity's OBJ import
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Glossiness", 0f);
        mat.SetFloat("_Metallic", 0f);
        meshRenderer.material = mat;
        
        // Ensure mesh is properly assigned
        if (meshFilter.mesh == null)
        {
            Debug.LogError("Failed to assign mesh to MeshFilter");
        }
        else
        {
            Debug.Log($"Mesh assigned successfully: {vertices.Count} vertices, {triangles.Count/3} triangles");
        }

        return parentObj;
    }
}