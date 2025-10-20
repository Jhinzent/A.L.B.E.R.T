using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GAEAMapCreator : MonoBehaviour
{
    [Header("UI Elements")]
    public Button importMapButton;
    public GameObject importPopup;
    public Button importImageButton;
    public Button importObjButton;
    public Button confirmImportButton;
    public Button closePopupButton;
    public TextMeshProUGUI statusText;
    
    [Header("Map Components")]
    public CameraMover cameraMover;
    
    private string selectedImagePath;
    private string selectedObjPath;
    private GameObject currentGaeaObject;
    private Texture2D currentMapTexture;
    
    void Start()
    {
        importMapButton.onClick.AddListener(OpenImportPopup);
        importImageButton.onClick.AddListener(ImportImage);
        importObjButton.onClick.AddListener(ImportObj);
        confirmImportButton.onClick.AddListener(ConfirmImport);
        closePopupButton.onClick.AddListener(CloseImportPopup);
        
        UpdateUI();
    }
    
    void UpdateUI()
    {
        confirmImportButton.interactable = !string.IsNullOrEmpty(selectedImagePath) && !string.IsNullOrEmpty(selectedObjPath);
    }
    
    void OpenImportPopup()
    {
        importPopup.SetActive(true);
        UpdateUI();
    }
    
    void CloseImportPopup()
    {
        importPopup.SetActive(false);
    }
    
    void ImportImage()
    {
        string[] filters = { "Image Files", "png,jpg,jpeg" };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Map Image", "", filters, false);
        
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            selectedImagePath = paths[0];
            UpdateStatus("Image selected: " + System.IO.Path.GetFileName(selectedImagePath));
            UpdateUI();
        }
    }
    
    void ImportObj()
    {
        string[] filters = { "3D Object", "obj" };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select 3D Object", "", filters, false);
        
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            selectedObjPath = paths[0];
            UpdateStatus("3D Object selected: " + System.IO.Path.GetFileName(selectedObjPath));
            UpdateUI();
        }
    }
    
    void ConfirmImport()
    {
        UpdateStatus("Loading map...");
        
        if (CreateGAEAMap())
        {
            importPopup.SetActive(false);
            UpdateStatus("Map loaded successfully");
        }
        else
        {
            UpdateStatus("Failed to load map");
        }
    }
    
    bool CreateGAEAMap()
    {
        // Clear existing map
        if (currentGaeaObject != null)
            DestroyImmediate(currentGaeaObject);
        
        // Load texture
        currentMapTexture = LoadTextureFromFile(selectedImagePath);
        if (currentMapTexture == null) return false;
        
        // Load 3D object
        currentGaeaObject = OBJLoader.LoadOBJFromFile(selectedObjPath);
        if (currentGaeaObject == null) return false;
        
        // Ensure ground layer and collider are properly set
        EnsureGroundSetup();
        
        // Apply texture to object
        ApplyTextureToObject();
        
        // Center and scale map
        CenterAndScaleMap();
        
        return true;
    }
    
    Texture2D LoadTextureFromFile(string path)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }
    
    void ApplyTextureToObject()
    {
        if (currentGaeaObject != null && currentMapTexture != null)
        {
            // Find renderer in child objects (like Unity's OBJ import)
            Renderer renderer = currentGaeaObject.GetComponentInChildren<Renderer>();
            
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.mainTexture = currentMapTexture;
                mat.SetFloat("_Glossiness", 0f); // Remove shine for terrain
                renderer.material = mat;
                
                Debug.Log($"Texture applied: {currentMapTexture.width}x{currentMapTexture.height}");
                
                // Check if mesh has UV coordinates
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.mesh != null)
                {
                    Vector2[] uvs = meshFilter.mesh.uv;
                    Debug.Log($"Mesh UV count: {uvs.Length}");
                }
            }
            else
            {
                Debug.LogWarning("No renderer found in GAEA object or its children");
            }
        }
    }
    
    void CenterAndScaleMap()
    {
        if (currentGaeaObject == null) return;
        
        // Get bounds before any scaling
        Bounds bounds = GetObjectBounds(currentGaeaObject);
        Debug.Log($"Original bounds: center={bounds.center}, size={bounds.size}");
        
        // Check if bounds are too small (precision issues)
        if (bounds.size.magnitude < 0.001f)
        {
            Debug.LogWarning("Bounds are extremely small, this may cause rendering issues");
            // Use a reasonable default scale instead of extreme scaling
            currentGaeaObject.transform.localScale = Vector3.one * 100f;
        }
        else
        {
            // Scale to larger size for tilted camera view
            float maxDimension = Mathf.Max(bounds.size.x, bounds.size.z);
            float targetSize = 500f; // Larger target for tilted camera
            float scale = targetSize / maxDimension;
            
            // Clamp scale to reasonable range to avoid precision issues
            scale = Mathf.Clamp(scale, 1f, 10000f);
            
            currentGaeaObject.transform.localScale = Vector3.one * scale;
            Debug.Log($"Map scaled to {scale}, bounds: {bounds.size}, target size: {targetSize}");
        }
        
        // Center the object with slight Z offset for camera view
        bounds = GetObjectBounds(currentGaeaObject); // Recalculate after scaling
        Vector3 centerOffset = -bounds.center;
        centerOffset.z += 50f; // Move slightly into positive Z for camera
        currentGaeaObject.transform.position = centerOffset;
        
        // Name the object for easy finding during save
        currentGaeaObject.name = "GAEAMap";
        
        Debug.Log($"Final position: {currentGaeaObject.transform.position}, scale: {currentGaeaObject.transform.localScale}");
    }
    
    Bounds GetObjectBounds(GameObject obj)
    {
        Renderer renderer = obj.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            Debug.Log($"Renderer bounds: center={bounds.center}, size={bounds.size}, extents={bounds.extents}");
            return bounds;
        }
        else
        {
            Debug.LogWarning("No renderer found for bounds calculation");
            return new Bounds(obj.transform.position, Vector3.one);
        }
    }
    
    void UpdateStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
        Debug.Log(message);
    }
    
    public GameObject GetCurrentMap() => currentGaeaObject;
    public Texture2D GetCurrentTexture() => currentMapTexture;
    public string GetCurrentImagePath() => selectedImagePath;
    public string GetCurrentObjPath() => selectedObjPath;
    
    void EnsureGroundSetup()
    {
        if (currentGaeaObject == null) 
        {
            Debug.LogWarning("EnsureGroundSetup: currentGaeaObject is null");
            return;
        }
        
        // Find the mesh object (child with renderer)
        Renderer renderer = currentGaeaObject.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
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
                    Debug.Log($"Ground setup complete: Layer {meshObj.layer}, Collider: {collider != null}, Mesh: {meshFilter.mesh != null}");
                }
            }
            else
            {
                Debug.Log($"Ground already set up: Layer {meshObj.layer}, Collider exists");
            }
        }
        else
        {
            Debug.LogWarning("EnsureGroundSetup: No renderer found in GAEA object");
        }
    }
}