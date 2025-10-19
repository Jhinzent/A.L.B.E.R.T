using UnityEngine;
using UnityEngine.UI;

public class MapTypeSelector : MonoBehaviour
{
    [Header("UI Elements")]
    public Toggle tileBasedToggle;
    public Toggle gaeaTerrainToggle;
    public InputField gaeaPathInput;
    public Button browseGAEAButton;
    public Button loadGAEAButton;
    public Text statusText;
    
    [Header("Map Managers")]
    public UnifiedMapManager unifiedMapManager;
    
    private MapConfiguration currentConfig;
    
    void Start()
    {
        currentConfig = new MapConfiguration();
        
        // Setup toggle listeners
        tileBasedToggle.onValueChanged.AddListener(OnTileBasedToggled);
        gaeaTerrainToggle.onValueChanged.AddListener(OnGAEAToggled);
        browseGAEAButton.onClick.AddListener(OnBrowseGAEAClicked);
        loadGAEAButton.onClick.AddListener(OnLoadGAEAClicked);
        
        // Default to tile-based
        tileBasedToggle.isOn = true;
        UpdateUIState();
    }
    
    private void OnTileBasedToggled(bool isOn)
    {
        if (isOn)
        {
            currentConfig.mapType = MapType.TileBased;
            gaeaTerrainToggle.isOn = false;
            UpdateUIState();
        }
    }
    
    private void OnGAEAToggled(bool isOn)
    {
        if (isOn)
        {
            currentConfig.mapType = MapType.GAEATerrain;
            tileBasedToggle.isOn = false;
            UpdateUIState();
        }
    }
    
    private void OnBrowseGAEAClicked()
    {
        string[] filters = { "Unity Prefab", "prefab", "Terrain Data", "asset" };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select GAEA Terrain", "", filters, false);
        
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string selectedPath = paths[0];
            gaeaPathInput.text = selectedPath;
            UpdateStatus("Terrain file selected: " + System.IO.Path.GetFileName(selectedPath));
        }
    }
    
    private void OnLoadGAEAClicked()
    {
        string path = gaeaPathInput.text.Trim();
        if (string.IsNullOrEmpty(path))
        {
            UpdateStatus("Please select a GAEA terrain file first");
            return;
        }
        
        UpdateStatus("Loading terrain...");
        
        if (LoadGAEATerrainFromFile(path))
        {
            currentConfig.gaeaTerrainPath = path;
            if (unifiedMapManager != null)
            {
                unifiedMapManager.InitializeMap(currentConfig);
            }
            UpdateStatus("Terrain loaded successfully");
        }
        else
        {
            UpdateStatus("Failed to load terrain file");
        }
    }
    
    private void UpdateUIState()
    {
        bool isGAEA = currentConfig.mapType == MapType.GAEATerrain;
        gaeaPathInput.interactable = isGAEA;
        browseGAEAButton.interactable = isGAEA;
        loadGAEAButton.interactable = isGAEA && !string.IsNullOrEmpty(gaeaPathInput.text);
    }
    
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log(message);
    }
    
    private bool LoadGAEATerrainFromFile(string filePath)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
            {
                return false;
            }
            
            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            string resourcesPath = Application.dataPath + "/Resources/GAEATerrains/";
            
            if (!System.IO.Directory.Exists(resourcesPath))
            {
                System.IO.Directory.CreateDirectory(resourcesPath);
            }
            
            string destinationPath = resourcesPath + fileName + System.IO.Path.GetExtension(filePath);
            System.IO.File.Copy(filePath, destinationPath, true);
            
            currentConfig.gaeaTerrainPath = "GAEATerrains/" + fileName;
            
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error loading GAEA terrain: " + e.Message);
            return false;
        }
    }
    
    public MapConfiguration GetCurrentConfiguration()
    {
        return currentConfig;
    }
    
    public void SetConfiguration(MapConfiguration config)
    {
        currentConfig = config;
        
        tileBasedToggle.isOn = config.mapType == MapType.TileBased;
        gaeaTerrainToggle.isOn = config.mapType == MapType.GAEATerrain;
        
        if (!string.IsNullOrEmpty(config.gaeaTerrainPath))
        {
            gaeaPathInput.text = config.gaeaTerrainPath;
        }
        
        UpdateUIState();
    }
}