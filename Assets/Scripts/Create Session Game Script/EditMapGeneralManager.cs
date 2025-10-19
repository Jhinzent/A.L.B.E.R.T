using UnityEngine;

public class EditMapGeneralManager : MonoBehaviour
{
    [Header("GAEA Map System")]
    public GAEAMapCreator gaeaMapCreator;
    public GameMasterMapLoader gameMasterMapLoader;
    
    // OLD SYSTEM - COMMENTED OUT
    // public GameMasterMapLoader gameMasterMapLoader;
    // public GameMasterMapManager terrainManager;
    public LayerMask gameMasterGroundLayer;

    private string loadSaveName;
    private string createSaveName;
    private string gameSessionSaveName;

    void Start()
    {
        LoadSessionIfExists();
    }

    private void LoadSessionIfExists()
    {
        string isLoaded = PlayerPrefs.GetString("EditSave", "");
        string isCreated = PlayerPrefs.GetString("CreateSave", "");
        string isGameSession = PlayerPrefs.GetString("StartSession", "");

        Debug.Log($"[EditMapGeneralManager] Checking PlayerPrefs - EditSave: '{isLoaded}', CreateSave: '{isCreated}', StartSession: '{isGameSession}'");

        if (!string.IsNullOrEmpty(isLoaded))
        {
            Debug.Log($"[EditMapGeneralManager] Loading existing save: {isLoaded}");
            loadSaveName = isLoaded;
            ClearPlayerPrefs();

            SaveData saveData = SaveSystem.LoadSession(isLoaded);
            if (saveData != null)
            {
                Debug.Log($"[EditMapGeneralManager] Save data loaded successfully. Objects count: {saveData.objects.Count}, Has GAEA data: {saveData.gaeaMapData != null}");
                
                // Use GameMasterMapLoader instead of separate GAEA loader
                GameMasterMapLoader mapLoader = FindObjectOfType<GameMasterMapLoader>();
                if (mapLoader != null)
                {
                    Debug.Log($"[EditMapGeneralManager] Using GameMasterMapLoader to load objects and GAEA map");
                    mapLoader.LoadObjectsIntoScene(saveData);
                }
                else
                {
                    Debug.LogError($"[EditMapGeneralManager] GameMasterMapLoader not found in scene!");
                    LoadObjects(saveData); // Fallback to old method
                }
            }
            else
            {
                Debug.LogError($"[EditMapGeneralManager] Failed to load save data for: {isLoaded}");
            }
        }

        if (!string.IsNullOrEmpty(isCreated))
        {
            Debug.Log($"[EditMapGeneralManager] Starting new creation session: {isCreated}");
            createSaveName = isCreated;
            ClearPlayerPrefs();
            
            // Start with empty GAEA map creator - no auto-generation needed
            Debug.Log("Starting new GAEA map creation session");
        }

        if (!string.IsNullOrEmpty(isGameSession))
        {
            Debug.Log($"[EditMapGeneralManager] Loading game session: {isGameSession}");
            gameSessionSaveName = isGameSession;
            ClearPlayerPrefs();

            SaveData saveData = SaveSystem.LoadSession(isGameSession);
            if (saveData != null)
            {
                Debug.Log($"[EditMapGeneralManager] Game session data loaded. Objects count: {saveData.objects.Count}, Has GAEA data: {saveData.gaeaMapData != null}");
                
                // Use GameMasterMapLoader instead of separate GAEA loader
                GameMasterMapLoader mapLoader = FindObjectOfType<GameMasterMapLoader>();
                if (mapLoader != null)
                {
                    Debug.Log($"[EditMapGeneralManager] Using GameMasterMapLoader to load objects and GAEA map");
                    mapLoader.LoadObjectsIntoScene(saveData);
                }
                else
                {
                    Debug.LogError($"[EditMapGeneralManager] GameMasterMapLoader not found in scene!");
                    LoadObjects(saveData); // Fallback to old method
                }
            }
            else
            {
                Debug.LogError($"[EditMapGeneralManager] Failed to load game session data for: {isGameSession}");
            }
        }

        if (string.IsNullOrEmpty(isLoaded) && string.IsNullOrEmpty(isCreated) && string.IsNullOrEmpty(isGameSession))
        {
            Debug.Log("[EditMapGeneralManager] No session to load - starting fresh");
        }
    }
    
    private void ClearPlayerPrefs()
    {
        PlayerPrefs.SetString("CreateSave", "");
        PlayerPrefs.SetString("EditSave", "");
        PlayerPrefs.SetString("StartSession", "");
    }

    public string getLoadSaveName()
    {
        return loadSaveName;
    }

    public string getCreateSaveName()
    {
        return createSaveName;
    }

    public string getGameSessionSaveName()
    {
        return gameSessionSaveName;
    }
    
    private void LoadObjects(SaveData saveData)
    {
        Debug.Log($"[EditMapGeneralManager] Loading {saveData.objects.Count} objects");
        
        foreach (ObjectData objData in saveData.objects)
        {
            Debug.Log($"[EditMapGeneralManager] Loading object: {objData.objectName}, prefab: {objData.prefabName}");
            
            if (!string.IsNullOrEmpty(objData.prefabName))
            {
                // Clean prefab name for loading
                string cleanPrefabName = objData.prefabName
                    .Replace("(Clone)", "")
                    .Replace(" Variant", "")
                    .Replace(" 1", "")
                    .Replace(" 2", "")
                    .Replace(" 3", "")
                    .Replace(" 4", "")
                    .Replace(" 5", "")
                    .Trim();
                
                Debug.Log($"[EditMapGeneralManager] Cleaned prefab name: '{objData.prefabName}' -> '{cleanPrefabName}'");
                
                // Load from Resources folder
                GameObject prefab = Resources.Load<GameObject>(cleanPrefabName);
                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab, objData.position, objData.rotation);
                    obj.name = objData.objectName;
                    Debug.Log($"[EditMapGeneralManager] Successfully instantiated: {objData.objectName} at {objData.position}");
                    
                    // Set up placeable item data
                    PlaceableItemInstance placeable = obj.GetComponent<PlaceableItemInstance>();
                    if (placeable != null)
                    {
                        // Initialize with saved data
                        placeable.Init(prefab, objData.itemType, objData.objectGivenName ?? "Object");
                        
                        if (!string.IsNullOrEmpty(objData.team))
                        {
                            placeable.setTeam(objData.team);
                        }
                        if (!string.IsNullOrEmpty(objData.objectGivenName))
                        {
                            placeable.SetName(objData.objectGivenName);
                        }
                        
                        // Load unit attributes if it's a unit
                        if (placeable.IsUnit())
                        {
                            placeable.SetProficiency(objData.proficiency);
                            placeable.SetFatigue(objData.fatigue);
                            placeable.SetCommsClarity(objData.commsClarity);
                            placeable.SetEquipment(objData.equipment);
                            Debug.Log($"[EditMapGeneralManager] Loaded unit attributes for: {objData.objectName}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[EditMapGeneralManager] No PlaceableItemInstance component found on: {objData.objectName}");
                    }
                }
                else
                {
                    Debug.LogError($"[EditMapGeneralManager] Failed to load prefab: '{objData.prefabName}' (cleaned: '{cleanPrefabName}'). Make sure the prefab exists in a Resources folder.");
                }
            }
            else
            {
                Debug.LogWarning($"[EditMapGeneralManager] Empty prefab name for object: {objData.objectName}");
            }
        }
    }
}