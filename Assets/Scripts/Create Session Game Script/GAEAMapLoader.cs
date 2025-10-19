using UnityEngine;

public class GAEAMapLoader : MonoBehaviour
{
    public GAEAMapCreator mapCreator;
    private GAEAMapData lastLoadedMapData;
    
    public void LoadGAEAMap(SaveData saveData)
    {
        Debug.Log($"[GAEAMapLoader] LoadGAEAMap called. SaveData null: {saveData == null}, GAEA data null: {saveData?.gaeaMapData == null}");
        
        if (saveData?.gaeaMapData == null) 
        {
            Debug.Log("[GAEAMapLoader] No GAEA map data to load");
            return;
        }
        
        GAEAMapData mapData = saveData.gaeaMapData;
        lastLoadedMapData = mapData; // Store for saving later
        
        Debug.Log($"[GAEAMapLoader] GAEA map data found - Image: {mapData.imagePath}, OBJ: {mapData.objPath}");
        
        // Load the map if paths are available
        if (!string.IsNullOrEmpty(mapData.imagePath) && !string.IsNullOrEmpty(mapData.objPath))
        {
            Debug.Log($"[GAEAMapLoader] Checking file existence...");
            bool imageExists = System.IO.File.Exists(mapData.imagePath);
            bool objExists = System.IO.File.Exists(mapData.objPath);
            
            Debug.Log($"[GAEAMapLoader] Image exists: {imageExists}, OBJ exists: {objExists}");
            
            if (imageExists && objExists)
            {
                Debug.Log($"[GAEAMapLoader] Both files exist, loading map...");
                LoadMapFromPaths(mapData);
            }
            else
            {
                Debug.LogError($"[GAEAMapLoader] Missing files - Image: {imageExists}, OBJ: {objExists}");
            }
        }
        else
        {
            Debug.LogWarning($"[GAEAMapLoader] Empty file paths in GAEA data");
        }
    }
    
    void LoadMapFromPaths(GAEAMapData mapData)
    {
        Debug.Log($"Loading GAEA map from: {mapData.objPath}");
        
        // Load texture
        Texture2D texture = LoadTextureFromFile(mapData.imagePath);
        if (texture == null) 
        {
            Debug.LogError($"Failed to load texture: {mapData.imagePath}");
            return;
        }
        
        // Load 3D object
        GameObject mapObject = OBJLoader.LoadOBJFromFile(mapData.objPath);
        if (mapObject == null) 
        {
            Debug.LogError($"Failed to load OBJ: {mapData.objPath}");
            return;
        }
        
        // Apply texture to child renderer (like Unity's OBJ import structure)
        Renderer renderer = mapObject.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.mainTexture = texture;
            mat.SetFloat("_Glossiness", 0f);
            mat.SetFloat("_Metallic", 0f);
            renderer.material = mat;
            
            // Ensure ground layer and collider are set (OBJLoader already handles this)
            GameObject meshObj = renderer.gameObject;
            meshObj.layer = 3; // Ground layer
            
            // Ensure collider exists
            MeshCollider collider = meshObj.GetComponent<MeshCollider>();
            if (collider == null)
            {
                collider = meshObj.AddComponent<MeshCollider>();
                MeshFilter meshFilter = meshObj.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    collider.sharedMesh = meshFilter.mesh;
                }
            }
        }
        
        // Apply saved transform
        mapObject.transform.position = mapData.mapPosition;
        mapObject.transform.localScale = mapData.mapScale;
        
        // Name the object for easy finding during save
        mapObject.name = "GAEAMap";
    }
    
    Texture2D LoadTextureFromFile(string path)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }
    
    public GAEAMapData GetLastLoadedMapData() => lastLoadedMapData;
}