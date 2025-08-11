
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
        List<TerrainTile> terrainTiles = new List<TerrainTile>();
        Vector3 offset = positionOffset ?? Vector3.zero;

        List<PlaceableItemInstance> loadedUnits = new List<PlaceableItemInstance>();

        foreach (ObjectData objData in saveData.objects)
        {
            string cleanName = objData.objectName.Replace("(Clone)", "").Replace(" Variant", "").Trim();

            if (prefabDictionary.TryGetValue(cleanName, out GameObject prefab))
            {
                Vector3 spawnPosition = objData.position + offset;
                GameObject newObj = Instantiate(prefab, spawnPosition, objData.rotation);
                newObj.tag = "Saveable";

                // Handle PlaceableItemInstance
                PlaceableItemInstance instance = newObj.GetComponent<PlaceableItemInstance>();
                if (instance != null)
                {
                    instance.Init(prefab, objData.itemType, objData.objectGivenName);

                    // Set the team for units
                    if (objData.itemType == PlaceableItem.ItemType.Unit && !string.IsNullOrEmpty(objData.team))
                    {
                        instance.setTeam(objData.team);
                    }

                    // Add to list if it's a unit (but do NOT send directly to TeamListManager)
                    if (objData.itemType == PlaceableItem.ItemType.Unit)
                    {
                        loadedUnits.Add(instance);
                    }
                }

                // Handle TerrainTile
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
            }
            else
            {
                Debug.LogError($"Prefab not found in dictionary: {cleanName}");
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
}
