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
        Debug.Log($"Loading OBJ file: {lines.Length} lines");

        int processedFaces = 0;
        int skippedFaces = 0;

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
                        
                        bool validFace = true;
                        
                        for (int i = 1; i < parts.Length; i++)
                        {
                            string[] indices = parts[i].Split('/');
                            
                            // Parse vertex index (required)
                            if (indices.Length > 0 && int.TryParse(indices[0], out int vIndex))
                            {
                                int adjustedVIndex = vIndex - 1;
                                if (adjustedVIndex >= 0 && adjustedVIndex < objVertices.Count)
                                {
                                    vertexIndices.Add(adjustedVIndex);
                                }
                                else
                                {
                                    Debug.LogWarning($"Invalid vertex index {vIndex} (adjusted: {adjustedVIndex}) - max: {objVertices.Count}");
                                    validFace = false;
                                    break;
                                }
                            }
                            else
                            {
                                validFace = false;
                                break;
                            }
                            
                            // Parse UV index (optional)
                            if (indices.Length > 1 && !string.IsNullOrEmpty(indices[1]) && int.TryParse(indices[1], out int uvIndex))
                            {
                                int adjustedUVIndex = uvIndex - 1;
                                if (adjustedUVIndex >= 0 && adjustedUVIndex < objUVs.Count)
                                {
                                    uvIndices.Add(adjustedUVIndex);
                                }
                                else
                                {
                                    uvIndices.Add(-1); // Mark as invalid
                                }
                            }
                            else
                            {
                                uvIndices.Add(-1);
                            }
                            
                            // Parse normal index (optional)
                            if (indices.Length > 2 && !string.IsNullOrEmpty(indices[2]) && int.TryParse(indices[2], out int nIndex))
                            {
                                int adjustedNIndex = nIndex - 1;
                                if (adjustedNIndex >= 0 && adjustedNIndex < objNormals.Count)
                                {
                                    normalIndices.Add(adjustedNIndex);
                                }
                                else
                                {
                                    normalIndices.Add(-1); // Mark as invalid
                                }
                            }
                            else
                            {
                                normalIndices.Add(-1);
                            }
                        }
                        
                        if (!validFace) 
                        {
                            skippedFaces++;
                            continue;
                        }
                        
                        processedFaces++;
                        
                        // Build unified vertex/UV/normal arrays
                        if (vertexIndices.Count >= 3)
                        {
                            // Handle triangles and quads (convert quads to triangles)
                            if (vertexIndices.Count == 4)
                            {
                                // Quad - create 2 triangles with proper UV mapping
                                int[] quadOrder = {0, 2, 1, 0, 3, 2}; // Reversed winding
                                
                                for (int i = 0; i < 6; i++)
                                {
                                    int idx = quadOrder[i];
                                    vertices.Add(objVertices[vertexIndices[idx]]);
                                    
                                    if (idx < uvIndices.Count && uvIndices[idx] >= 0)
                                        uvs.Add(objUVs[uvIndices[idx]]);
                                    else
                                        uvs.Add(Vector2.zero);
                                        
                                    if (idx < normalIndices.Count && normalIndices[idx] >= 0)
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
                                    
                                    if (idx < uvIndices.Count && uvIndices[idx] >= 0)
                                        uvs.Add(objUVs[uvIndices[idx]]);
                                    else
                                        uvs.Add(Vector2.zero);
                                        
                                    if (idx < normalIndices.Count && normalIndices[idx] >= 0)
                                        normals.Add(objNormals[normalIndices[idx]]);
                                    else
                                        normals.Add(Vector3.up);
                                        
                                    triangles.Add(vertices.Count - 1);
                                }
                            }
                            else
                            {
                                // Handle n-gons by fan triangulation
                                for (int i = 1; i < vertexIndices.Count - 1; i++)
                                {
                                    int[] fanOrder = {0, i + 1, i}; // Reversed winding
                                    
                                    for (int j = 0; j < 3; j++)
                                    {
                                        int idx = fanOrder[j];
                                        vertices.Add(objVertices[vertexIndices[idx]]);
                                        
                                        if (idx < uvIndices.Count && uvIndices[idx] >= 0)
                                            uvs.Add(objUVs[uvIndices[idx]]);
                                        else
                                            uvs.Add(Vector2.zero);
                                            
                                        if (idx < normalIndices.Count && normalIndices[idx] >= 0)
                                            normals.Add(objNormals[normalIndices[idx]]);
                                        else
                                            normals.Add(Vector3.up);
                                            
                                        triangles.Add(vertices.Count - 1);
                                    }
                                }
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
        
        // Enable 32-bit indices for large meshes
        if (vertices.Count > 65535)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Debug.Log($"Enabled 32-bit indices for large mesh with {vertices.Count} vertices");
        }
        
        if (vertices.Count == 0)
        {
            Debug.LogError("No vertices found in OBJ file");
            return null;
        }
        
        // Check Unity's mesh vertex limit
        if (vertices.Count > 65535)
        {
            Debug.LogWarning($"Mesh has {vertices.Count} vertices, exceeding Unity's 16-bit index limit (65535). This may cause rendering issues.");
        }
        
        // Scaling already applied during vertex parsing to match Unity's import
        Vector3[] vertexArray = vertices.ToArray();
        mesh.vertices = vertexArray;
        
        // Calculate actual bounds from vertices
        if (vertexArray.Length > 0)
        {
            Vector3 min = vertexArray[0];
            Vector3 max = vertexArray[0];
            foreach (Vector3 v in vertexArray)
            {
                min = Vector3.Min(min, v);
                max = Vector3.Max(max, v);
            }
            Vector3 size = max - min;
            Debug.Log($"Actual vertex bounds: min={min}, max={max}, size={size}");
        }
        
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
        Debug.Log($"OBJ parsing complete: {objVertices.Count} vertices, {objUVs.Count} UVs, {objNormals.Count} normals");
        Debug.Log($"Final mesh: {vertices.Count} vertices, {triangles.Count/3} triangles, {processedFaces} faces processed, {skippedFaces} faces skipped");
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