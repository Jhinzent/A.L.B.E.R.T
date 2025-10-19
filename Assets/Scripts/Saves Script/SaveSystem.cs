using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    private static string savePath => Application.persistentDataPath + "/saves/";

    public static void SaveSession(string saveName)
    {
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        SaveData saveData = new SaveData { saveName = saveName };
        
        // Save GAEA map data - check both creator and loader
        GAEAMapCreator mapCreator = FindObjectOfType<GAEAMapCreator>();
        GAEAMapLoader mapLoader = FindObjectOfType<GAEAMapLoader>();
        
        if (mapCreator != null)
        {
            GameObject currentMap = mapCreator.GetCurrentMap();
            if (currentMap != null)
            {
                saveData.gaeaMapData = new GAEAMapData
                {
                    imagePath = mapCreator.GetCurrentImagePath(),
                    objPath = mapCreator.GetCurrentObjPath(),
                    mapPosition = currentMap.transform.position,
                    mapScale = currentMap.transform.localScale
                };
            }
        }
        
        // If no creator found, try to find GameMasterMapLoader or existing GAEA map
        if (saveData.gaeaMapData == null)
        {
            GameMasterMapLoader gameMasterLoader = FindObjectOfType<GameMasterMapLoader>();
                
            GameObject existingMap = GameObject.Find("GAEAMap");
            if (existingMap != null)
            {
                if (gameMasterLoader != null && gameMasterLoader.GetLastLoadedMapData() != null)
                {
                    GAEAMapData lastData = gameMasterLoader.GetLastLoadedMapData();
                    saveData.gaeaMapData = new GAEAMapData
                    {
                        imagePath = lastData.imagePath,
                        objPath = lastData.objPath,
                        mapPosition = existingMap.transform.position,
                        mapScale = existingMap.transform.localScale
                    };
                }
                else
                {
                    // Fallback: save map transform but no file paths
                    saveData.gaeaMapData = new GAEAMapData
                    {
                        imagePath = "",
                        objPath = "",
                        mapPosition = existingMap.transform.position,
                        mapScale = existingMap.transform.localScale
                    };
                }
            }
        }

        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            if (obj.CompareTag("Saveable"))
            {
                ObjectData objectData = new ObjectData
                {
                    objectName = obj.name,
                    position = obj.transform.position,
                    rotation = obj.transform.rotation,
                    // terrainType = null, // COMMENTED OUT - old tile system
                    prefabName = null   // default null unless placeable
                };

                // If it's a terrain tile, save the terrain type - COMMENTED OUT FOR GAEA-ONLY
                /*
                TerrainTile terrainTile = obj.GetComponent<TerrainTile>();
                if (terrainTile != null)
                {
                    objectData.terrainType = terrainTile.GetTerrainType().ToString();
                }
                */

                // If it has a PlaceableItemInstance, save extra info
                PlaceableItemInstance placeable = obj.GetComponent<PlaceableItemInstance>();
                if (placeable != null)
                {
                    if (placeable.OriginalPrefab != null)
                    {
                        string prefabName = placeable.OriginalPrefab.name;
                        // Remove common Unity suffixes and numbering
                        prefabName = prefabName.Replace("(Clone)", "")
                                              .Replace(" Variant", "")
                                              .Replace(" 1", "")
                                              .Replace(" 2", "")
                                              .Replace(" 3", "")
                                              .Replace(" 4", "")
                                              .Replace(" 5", "")
                                              .Trim();
                        objectData.prefabName = prefabName;
                    }

                    objectData.itemType = placeable.ItemType;

                    // Save team and attributes for units
                    if (placeable.IsUnit())
                    {
                        objectData.team = placeable.getTeam();
                        objectData.objectGivenName = placeable.getName();
                        objectData.proficiency = placeable.GetProficiency();
                        objectData.fatigue = placeable.GetFatigue();
                        objectData.commsClarity = placeable.GetCommsClarity();
                        objectData.equipment = placeable.GetEquipment();
                    }
                    else 
                    {
                        objectData.objectGivenName = "Object";
                    }
                }



                saveData.objects.Add(objectData);
            }
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath + saveName + ".json", json);
        Debug.Log("Saved session to: " + savePath + saveName + ".json");
    }

    public static List<string> GetAllSaves()
    {
        if (!Directory.Exists(savePath)) return new List<string>();

        List<string> saves = new List<string>();
        foreach (string file in Directory.GetFiles(savePath, "*.json"))
        {
            saves.Add(Path.GetFileNameWithoutExtension(file));
        }

        return saves;
    }

    public static SaveData LoadSession(string saveName)
    {
        string filePath = savePath + saveName + ".json";
        Debug.Log($"[SaveSystem] Attempting to load save file: {filePath}");
        
        if (File.Exists(filePath))
        {
            Debug.Log($"[SaveSystem] Save file exists, reading content...");
            string json = File.ReadAllText(filePath);
            Debug.Log($"[SaveSystem] File content length: {json.Length} characters");
            
            SaveData result = JsonUtility.FromJson<SaveData>(json);
            if (result != null)
            {
                Debug.Log($"[SaveSystem] Successfully parsed save data. Save name: {result.saveName}, Objects: {result.objects?.Count ?? 0}, Has GAEA data: {result.gaeaMapData != null}");
            }
            else
            {
                Debug.LogError($"[SaveSystem] Failed to parse JSON from file: {filePath}");
            }
            return result;
        }

        Debug.LogError($"[SaveSystem] Save file not found: {filePath}");
        return null;
    }
}