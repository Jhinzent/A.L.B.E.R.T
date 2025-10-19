using UnityEngine;
using UnityEngine.UI;

public class MapModeManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button mapModeButton;
    public GameObject mapModePopup;
    public Button regularMapButton;
    public Button gaeaMapButton;
    public Button importMapButton;
    public GameObject importPopup;
    public Button importImageButton;
    public Button importObjButton;
    public Button confirmImportButton;
    
    [Header("UI Panels")]
    public GameObject regularMapUI;
    public GameObject gaeaMapUI;
    
    [Header("Map Components")]
    public GameMasterMapManager regularMapManager;
    public CameraMover cameraMover;
    
    private MapMode currentMode = MapMode.GAEA;
    private string selectedImagePath;
    private string selectedObjPath;
    private GameObject currentGaeaObject;
    private Sprite currentMapSprite;
    
    // Temporary save data
    private SaveData regularMapData;
    private GAEAMapData gaeaMapData;
    
    void Start()
    {
        mapModeButton.onClick.AddListener(OpenMapModePopup);
        regularMapButton.onClick.AddListener(SwitchToRegularMode);
        gaeaMapButton.onClick.AddListener(SwitchToGAEAMode);
        importMapButton.onClick.AddListener(OpenImportPopup);
        importImageButton.onClick.AddListener(ImportImage);
        importObjButton.onClick.AddListener(ImportObj);
        confirmImportButton.onClick.AddListener(ConfirmImport);
        
        // Start in GAEA mode
        SwitchToGAEAMode();
    }
    
    void OpenMapModePopup()
    {
        mapModePopup.SetActive(true);
    }
    
    void SwitchToRegularMode()
    {
        SaveCurrentProgress();
        currentMode = MapMode.Regular;
        mapModePopup.SetActive(false);
        UpdateUI();
        LoadRegularMapData();
    }
    
    void SwitchToGAEAMode()
    {
        SaveCurrentProgress();
        currentMode = MapMode.GAEA;
        mapModePopup.SetActive(false);
        UpdateUI();
        // LoadGAEAMapData(); // COMMENTED OUT
    }
    
    void UpdateUI()
    {
        regularMapUI.SetActive(currentMode == MapMode.Regular);
        gaeaMapUI.SetActive(currentMode == MapMode.GAEA);
        importMapButton.interactable = currentMode == MapMode.GAEA;
    }
    
    void OpenImportPopup()
    {
        importPopup.SetActive(true);
        confirmImportButton.interactable = !string.IsNullOrEmpty(selectedImagePath) && !string.IsNullOrEmpty(selectedObjPath);
    }
    
    void ImportImage()
    {
        string[] filters = { "Image Files", "png,jpg,jpeg" };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Map Image", "", filters, false);
        
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            selectedImagePath = paths[0];
            confirmImportButton.interactable = !string.IsNullOrEmpty(selectedObjPath);
        }
    }
    
    void ImportObj()
    {
        string[] filters = { "3D Object", "obj" };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select 3D Object", "", filters, false);
        
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            selectedObjPath = paths[0];
            confirmImportButton.interactable = !string.IsNullOrEmpty(selectedImagePath);
        }
    }
    
    void ConfirmImport()
    {
        if (LoadAndSetupGAEAMap())
        {
            importPopup.SetActive(false);
            selectedImagePath = "";
            selectedObjPath = "";
        }
    }
    
    bool LoadAndSetupGAEAMap()
    {
        // Load sprite
        currentMapSprite = LoadSpriteFromFile(selectedImagePath);
        if (currentMapSprite == null) return false;
        
        // Load 3D object
        currentGaeaObject = LoadObjFromFile(selectedObjPath);
        if (currentGaeaObject == null) return false;
        
        // Apply sprite as texture
        ApplySpriteToObject();
        
        // Center and scale
        CenterAndScaleMap();
        
        return true;
    }
    
    Sprite LoadSpriteFromFile(string path)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
    }
    
    GameObject LoadObjFromFile(string path)
    {
        return OBJLoader.LoadOBJFromFile(path);
    }
    
    void ApplySpriteToObject()
    {
        if (currentGaeaObject != null && currentMapSprite != null)
        {
            Renderer renderer = currentGaeaObject.GetComponent<Renderer>();
            if (renderer == null)
                renderer = currentGaeaObject.AddComponent<MeshRenderer>();
            
            Material mat = new Material(Shader.Find("Standard"));
            mat.mainTexture = currentMapSprite.texture;
            renderer.material = mat;
        }
    }
    
    void CenterAndScaleMap()
    {
        if (currentGaeaObject == null || cameraMover == null) return;
        
        Bounds bounds = GetObjectBounds(currentGaeaObject);
        Vector3 cameraCenter = cameraMover.transform.position;
        
        // Center the object
        currentGaeaObject.transform.position = cameraCenter - bounds.center;
        
        // Scale to fit camera bounds
        float cameraSize = Camera.main.orthographicSize * 2f;
        float maxDimension = Mathf.Max(bounds.size.x, bounds.size.z);
        float scale = (cameraSize * 0.8f) / maxDimension;
        currentGaeaObject.transform.localScale = Vector3.one * scale;
    }
    
    Bounds GetObjectBounds(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        return renderer != null ? renderer.bounds : new Bounds(obj.transform.position, Vector3.one);
    }
    
    void SaveCurrentProgress()
    {
        // COMMENTED OUT - OLD SYSTEM
        /*
        if (currentMode == MapMode.Regular)
        {
            regularMapData = CreateRegularMapSaveData();
        }
        else
        {
            gaeaMapData = CreateGAEAMapSaveData();
        }
        */
    }
    
    void LoadRegularMapData()
    {
        if (regularMapData != null)
        {
            // Load regular map tiles
            regularMapManager.RebuildGridFromTiles(GetTilesFromSaveData(regularMapData));
        }
    }
    
    // COMMENTED OUT - OLD SYSTEM
    /*
    void LoadGAEAMapData()
    {
        if (gaeaMapData != null)
        {
            // Restore GAEA map
            if (gaeaMapData.gaeaObject != null)
            {
                currentGaeaObject = gaeaMapData.gaeaObject;
                currentMapSprite = gaeaMapData.mapSprite;
            }
        }
    }
    
    SaveData CreateRegularMapSaveData()
    {
        // Create save data from current regular map state
        return new SaveData { mapConfig = new MapConfiguration { mapType = MapType.TileBased } };
    }
    
    GAEAMapData CreateGAEAMapSaveData()
    {
        return new GAEAMapData
        {
            gaeaObject = currentGaeaObject,
            mapSprite = currentMapSprite,
            imagePath = selectedImagePath,
            objPath = selectedObjPath
        };
    }
    */
    
    System.Collections.Generic.List<TerrainTile> GetTilesFromSaveData(SaveData data)
    {
        // Convert save data back to tiles
        return new System.Collections.Generic.List<TerrainTile>();
    }
    
    public MapMode GetCurrentMode() => currentMode;
    public SaveData GetRegularMapData() => regularMapData;
    public GAEAMapData GetGAEAMapData() => gaeaMapData;
}

public enum MapMode
{
    Regular,
    GAEA
}

// GAEAMapData moved to SaveDataStructure.cs