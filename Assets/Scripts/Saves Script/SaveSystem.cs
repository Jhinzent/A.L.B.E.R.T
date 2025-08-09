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

        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            if (obj.CompareTag("Saveable"))
            {
                ObjectData objectData = new ObjectData
                {
                    objectName = obj.name,
                    position = obj.transform.position,
                    rotation = obj.transform.rotation,
                    terrainType = null, // default null if not a terrain
                    prefabName = null   // default null unless placeable
                };

                // If it's a terrain tile, save the terrain type
                TerrainTile terrainTile = obj.GetComponent<TerrainTile>();
                if (terrainTile != null)
                {
                    objectData.terrainType = terrainTile.GetTerrainType().ToString();
                }

                // If it has a PlaceableItemInstance, save extra info
                PlaceableItemInstance placeable = obj.GetComponent<PlaceableItemInstance>();
                if (placeable != null)
                {
                    if (placeable.OriginalPrefab != null)
                        objectData.prefabName = placeable.OriginalPrefab.name;

                    objectData.itemType = placeable.ItemType;

                    // 🆕 Only save team if it's a unit
                    if (placeable.IsUnit())
                    {
                        objectData.team = placeable.getTeam();
                        objectData.objectGivenName = placeable.getName();
                    }
                    else objectData.objectGivenName = "Object";
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
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<SaveData>(json);
        }

        Debug.LogError("Save file not found: " + filePath);
        return null;
    }
}