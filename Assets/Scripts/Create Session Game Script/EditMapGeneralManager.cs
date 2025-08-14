using UnityEngine;

public class EditMapGeneralManager : MonoBehaviour
{
    [Header("Maps")]
    public GameMasterMapLoader gameMasterMapLoader;
    public GameMasterMapManager terrainManager;
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

        if (!string.IsNullOrEmpty(isLoaded))
        {
            loadSaveName = isLoaded;

            PlayerPrefs.SetString("CreateSave", "");
            PlayerPrefs.SetString("EditSave", "");
            PlayerPrefs.SetString("StartSession", "");

            SaveData saveData = SaveSystem.LoadSession(isLoaded);
            if (saveData != null)
            {
                gameMasterMapLoader.LoadObjectsIntoScene(saveData);
            }
        }

        if (!string.IsNullOrEmpty(isCreated))
        {
            createSaveName = isCreated;

            PlayerPrefs.SetString("CreateSave", "");
            PlayerPrefs.SetString("EditSave", "");
            PlayerPrefs.SetString("StartSession", "");

            terrainManager.GenerateGrid();
        }

        if (!string.IsNullOrEmpty(isGameSession))
        {
            gameSessionSaveName = isGameSession;

            PlayerPrefs.SetString("CreateSave", "");
            PlayerPrefs.SetString("EditSave", "");
            PlayerPrefs.SetString("StartSession", "");

            SaveData saveData = SaveSystem.LoadSession(isGameSession);
            if (saveData != null)
            {
                gameMasterMapLoader.LoadObjectsIntoScene(saveData, Vector3.zero);
            }
        }
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
}