
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameMasterMapLoader : MonoBehaviour
{
    [SerializeField] private List<GameObject> availablePrefabs; // Assign prefabs in Inspector
    [SerializeField] private TeamList teamListManager; // assign in Inspector
    public ObjectPlacer objectPlacer;
    public LayerMask gameMasterMapLayer;
    private Dictionary<string, GameObject> prefabDictionary;
    public GameMasterMapManager terrainManager;
    public UnifiedMapManager unifiedMapManager;
    
    // GAEA Map Loading
    private GAEAMapData lastLoadedMapData;

    void Awake()
    {
        prefabDictionary = new Dictionary<string, GameObject>();

        foreach (GameObject prefab in availablePrefabs)
        {
            string cleanName = prefab.name.Replace(" Variant", "").Trim(); // Ensure name matches cleaned version
            prefabDictionary[cleanName] = prefab;
        }
    }

    public void LoadObjectsIntoScene(SaveData saveData, Vector3? positionOffset = null)
    {
        // Load GAEA map if present
        LoadGAEAMap(saveData);
        
        // Initialize map based on save data - COMMENTED OUT FOR GAEA-ONLY
        /*
        if (unifiedMapManager != null)
        {
            unifiedMapManager.LoadMapFromSave(saveData);
        }
        */
        
        List<TerrainTile> terrainTiles = new List<TerrainTile>();
        Vector3 offset = positionOffset ?? Vector3.zero;

        List<PlaceableItemInstance> loadedUnits = new List<PlaceableItemInstance>();

        foreach (ObjectData objData in saveData.objects)
        {
            // Try multiple name variations to find the prefab
            GameObject prefab = FindPrefabByName(objData.prefabName ?? objData.objectName);
            
            if (prefab != null)
            {
                Vector3 spawnPosition = objData.position + offset;
                GameObject newObj = Instantiate(prefab, spawnPosition, objData.rotation);
                newObj.tag = "Saveable";

                // Handle PlaceableItemInstance
                PlaceableItemInstance instance = newObj.GetComponent<PlaceableItemInstance>();
                if (instance != null)
                {
                    instance.Init(prefab, objData.itemType, objData.objectGivenName ?? "Object");

                    // Set the team for units
                    if (objData.itemType == PlaceableItem.ItemType.Unit && !string.IsNullOrEmpty(objData.team))
                    {
                        instance.setTeam(objData.team);
                        
                        // Load unit attributes
                        instance.SetProficiency(objData.proficiency);
                        instance.SetFatigue(objData.fatigue);
                        instance.SetCommsClarity(objData.commsClarity);
                        instance.SetEquipment(objData.equipment);
                    }

                    // Ensure ViewRangeVisualizer is hidden for loaded units
                    if (objData.itemType == PlaceableItem.ItemType.Unit)
                    {
                        var visualizer = newObj.GetComponent<ViewRangeVisualizer>();
                        if (visualizer != null)
                        {
                            visualizer.ClearRing();
                        }
                        loadedUnits.Add(instance);
                    }
                }

                // Handle TerrainTile - COMMENTED OUT FOR GAEA-ONLY
                /*
                TerrainTile terrainTile = newObj.GetComponent<TerrainTile>();
                if (terrainTile != null)
                {
                    if (!string.IsNullOrEmpty(objData.terrainType) &&
                        Enum.TryParse(objData.terrainType, out TerrainTile.TerrainType terrainType))
                    {
                        terrainTile.SetTerrainType(terrainType);
                    }
                    terrainTiles.Add(terrainTile);
                }
                */
            }
            else
            {
                Debug.LogError($"Prefab not found: {objData.prefabName ?? objData.objectName}");
            }
        }

        // Rebuild the terrain grid with all loaded terrain tiles
        if (terrainTiles.Count > 0)
        {
            terrainManager.RebuildGridFromTiles(terrainTiles);
        }
        else
        {
            Debug.LogWarning("No terrain tiles found to rebuild grid.");
        }

        // Instead of populating the team list here, send the loaded units to the ObjectPlacer
        if (objectPlacer != null && loadedUnits.Count > 0)
        {
            foreach (var unit in loadedUnits)
            {
                objectPlacer.AddUnit(unit);  // You need to implement AddUnit in ObjectPlacer
            }
            objectPlacer.UpdateTeamList();  // Assuming you have a method that updates the team list UI
        }
        else if (objectPlacer == null)
        {
            Debug.LogWarning("ObjectPlacer reference is not set in GameMasterMapLoader.");
        }
    }
    
    private GameObject FindPrefabByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        
        // Clean the name
        string cleanName = name.Replace("(Clone)", "")
                              .Replace(" Variant", "")
                              .Replace(" 1", "")
                              .Replace(" 2", "")
                              .Replace(" 3", "")
                              .Replace(" 4", "")
                              .Replace(" 5", "")
                              .Trim();
        
        // Try exact match first
        if (prefabDictionary.TryGetValue(cleanName, out GameObject prefab))
            return prefab;
            
        // Try original name
        if (prefabDictionary.TryGetValue(name, out prefab))
            return prefab;
            
        // Try partial matches
        foreach (var kvp in prefabDictionary)
        {
            if (kvp.Key.Contains(cleanName) || cleanName.Contains(kvp.Key))
                return kvp.Value;
        }
        
        return null;
    }
    
    public void LoadGAEAMap(SaveData saveData)
    {
        if (saveData?.gaeaMapData == null) return;
        
        GAEAMapData mapData = saveData.gaeaMapData;
        lastLoadedMapData = mapData;
        
        if (!string.IsNullOrEmpty(mapData.imagePath) && !string.IsNullOrEmpty(mapData.objPath))
        {
            if (System.IO.File.Exists(mapData.imagePath) && System.IO.File.Exists(mapData.objPath))
            {
                LoadMapFromPaths(mapData);
            }
        }
    }
    
    private void LoadMapFromPaths(GAEAMapData mapData)
    {
        // Load texture
        Texture2D texture = LoadTextureFromFile(mapData.imagePath);
        if (texture == null) return;
        
        // Load 3D object
        GameObject mapObject = OBJLoader.LoadOBJFromFile(mapData.objPath);
        if (mapObject == null) return;
        
        // Apply texture
        Renderer renderer = mapObject.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.mainTexture = texture;
            mat.SetFloat("_Glossiness", 0f);
            renderer.material = mat;
            
            // Ensure ground setup
            GameObject meshObj = renderer.gameObject;
            meshObj.layer = 3;
            
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
        
        // Position at origin for GameSession scene
        mapObject.transform.position = Vector3.zero;
        mapObject.transform.localScale = mapData.mapScale;
        mapObject.transform.localScale = mapData.mapScale;
        mapObject.name = "GAEAMap";
    }
    
    private Texture2D LoadTextureFromFile(string path)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }
    
    public GAEAMapData GetLastLoadedMapData() => lastLoadedMapData;
}
