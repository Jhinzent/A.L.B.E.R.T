using System.Collections.Generic;
using UnityEngine;

public class GeneralSessionManager : MonoBehaviour
{
    [Header("Game Master")]
    public Camera GameMasterCamera;
    public Canvas GameMasterCanvas;
    public LayerMask gameMasterGroundLayer;

    [Header("Maps")]
    public List<Transform> playerMaps = new List<Transform>();

    [Header("Players")]
    public List<Camera> playerCameras = new List<Camera>();
    public List<Canvas> playerCanvases = new List<Canvas>();
    public LayerMask playerGroundLayer; // assuming all players use the same ground layer

    [Header("References")]
    public ObjectPlacer objectPlacer;
    public UiControllerCreateSessionGameScene uiControllerCreateSessionGameScene;
    public PlayerMapLoader playerMapLoader;
    public GameMasterMapLoader gameMasterMapLoader;
    public GameMasterMapManager terrainManager;
    public RadioButtonUI teamChangeRadioButton;
    public ReportScrollViewManager reportScrollViewManager;
    public PlayerReportScrollViewManager playerReportScrollViewManager;
    public ReportScrollViewManagerGameMasterIncoming reportScrollViewManagerGameMasterIncoming;
    public PlayerReportOutputScrollViewManager playerReportOutputScrollViewManager;
    public ActionScrollViewManager actionScrollViewManager;

    [Header("Debug / Testing")]
    public bool allowKeyboardAdvance = true; // press Space to switch to next
    public bool verboseLogs = true; // toggle lots of logs

    private bool reloadFlag;
    private int playerAmmount;
    private int currentIndex = 0; // 0 = GameMaster, 1..N = Players
    private string currentSaveName;
    private string createSaveName;
    private string loadSaveName;
    private string gameSessioSaveName;
    private string gameTime = "01/01/2024 00:00";

    void Start()
    {
        // Read player amount from previous scene
        playerAmmount = SceneData.playerAmmount;
        SceneData.playerAmmount = 0; // clear after reading

        // Debug.Log($"[GSM] Start() - SceneData.playerAmmount read = {playerAmmount}");

        // Validate lists vs requested player amount
        int availablePlayers = Mathf.Min(
            playerCameras != null ? playerCameras.Count : 0,
            playerCanvases != null ? playerCanvases.Count : 0
        );

        if (playerAmmount > availablePlayers)
        {
            // Debug.LogWarning($"[GSM] Requested playerAmmount = {playerAmmount}, but only {availablePlayers} player camera/canvas pairs are assigned. Clamping to {availablePlayers}.");
            playerAmmount = availablePlayers;
        }

        if (playerAmmount < 0) playerAmmount = 0;

        // Debug.Log($"[GSM] Starting with playerAmmount (effective) = {playerAmmount}, availablePlayers = {availablePlayers}");

        LoadSessionIfExists();
        // populate radio options
        SetPlayerOptions();

        LoadAllPlayerMaps();

        // Start in Game Master view
        currentIndex = 0;
        SwitchToIndex(0);
        reloadFlag = false;
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
            gameSessioSaveName = isGameSession;
            PlayerPrefs.SetString("CreateSave", "");
            PlayerPrefs.SetString("EditSave", "");
            PlayerPrefs.SetString("StartSession", "");

            SaveData saveData = SaveSystem.LoadSession(isGameSession);

            if (saveData != null)
            {
                gameMasterMapLoader.LoadObjectsIntoScene(saveData, new Vector3(0, 0, 0));
            }

            objectPlacer.setGroundLayerForPlacement(gameMasterGroundLayer);

        }
    }

    public string getCreateSaveName()
    {
        return createSaveName;
    }

    public string getLoadSaveName()
    {
        return loadSaveName;
    }

    public string getGameSessionSaveName()
    {
        return gameSessioSaveName;
    }

    public void SetGameTime(string time)
    {
        gameTime = time;
    }

    public string GetGameTime()
    {
        return gameTime;
    }

    public void UpdateAllPlayerTimes(string time)
    {
        SetGameTime(time);
        
        // Update GameMaster UI
        if (uiControllerCreateSessionGameScene != null)
        {
            uiControllerCreateSessionGameScene.SetGameTime(time);
        }
        
        // Update all player UIs
        if (playerCanvases != null)
        {
            for (int i = 0; i < playerCanvases.Count && i < playerAmmount; i++)
            {
                var playerUI = playerCanvases[i].GetComponent<UiControllerMainGameScene>();
                if (playerUI != null)
                {
                    playerUI.UpdateTimeDisplay(time);
                }
            }
        }
    }

    public string ValidatePassphrase(string passphrase)
    {
        int nextPlayerIndex = currentIndex + 1;
        
        if (nextPlayerIndex > playerAmmount)
        {
            // Next is GameMaster (wrapping around)
            if (passphrase == "GameMaster")
            {
                SwitchToNext();
                return "Successful";
            }
            return "Not Successful";
        }
        else
        {
            // Next is a player
            string expectedPassphrase = $"Player{nextPlayerIndex}";
            if (passphrase == expectedPassphrase)
            {
                SwitchToNext();
                return "Successful";
            }
            return "Not Successful";
        }
    }

    void LoadAllPlayerMaps()
    {
        if (playerMapLoader == null)
        {
            Debug.LogWarning("[GSM] playerMapLoader is not assigned, cannot load player maps");
            return;
        }

        playerMapLoader.CreatePlayerMaps(playerAmmount);
    }

    void ReloadPlayerMaps()
    {
        if (playerMapLoader == null)
        {
            Debug.LogWarning("[GSM] playerMapLoader is not assigned, cannot reload player maps");
            return;
        }

        string currentSave = getCurrentSaveName();

        SaveSystem.SaveSession(getCurrentSaveName());

        if (!string.IsNullOrEmpty(currentSave))
        {
            SaveData saveData = SaveSystem.LoadSession(currentSave);
            if (saveData != null)
            {
                playerMapLoader.ReloadPlayerMapsFromSave(saveData, playerAmmount);
                Debug.Log($"[GSM] Player maps reloaded from save: {currentSave}");
            }
        }
    }

    string getCurrentSaveName()
    {
        if (!string.IsNullOrEmpty(gameSessioSaveName)) return gameSessioSaveName;
        if (!string.IsNullOrEmpty(loadSaveName)) return loadSaveName;
        if (!string.IsNullOrEmpty(createSaveName)) return createSaveName;
        return "";
    }

    void Awake()
    {
        // Disable all player canvases
        if (playerCanvases != null)
        {
            foreach (var cvs in playerCanvases)
            {
                if (cvs != null)
                    cvs.gameObject.SetActive(false);
            }
        }

        // Disable GameMaster canvas (we will enable in Start)
        if (GameMasterCanvas != null)
            GameMasterCanvas.gameObject.SetActive(false);

        // --- AUDIO: ensure no AudioListener is accidentally enabled at scene start ---
        // Disable AudioListeners on all cameras here so Unity doesn't warn before Start runs.
        if (GameMasterCamera != null)
            ToggleAudioListener(GameMasterCamera, false);

        if (playerCameras != null)
        {
            foreach (var cam in playerCameras)
            {
                if (cam != null)
                    ToggleAudioListener(cam, false);
            }
        }
    }

    private void PositionCameraOverMap(Camera cam, Transform mapTransform)
    {
        if (cam == null) return;

        Vector3 basePosition = Vector3.zero;

        if (mapTransform != null)
            basePosition = mapTransform.position;

        Vector3 newPos = new Vector3(basePosition.x, basePosition.y + 80f, basePosition.z);
        cam.transform.position = newPos;
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // look straight down

        if (verboseLogs)
        {
            string mapName = mapTransform != null ? mapTransform.name : "Origin (0,0,0)";
            /*Debug.Log($"[GSM] PositionCameraOverMap: Moving camera '{cam.name}' over map '{mapName}'");
            Debug.Log($"[GSM] Map position: {basePosition}");
            Debug.Log($"[GSM] New camera position set to: {newPos}");
            Debug.Log($"[GSM] Camera rotation set to: {cam.transform.rotation.eulerAngles}"); */
        }
    }

    void Update()
    {
        if (allowKeyboardAdvance && Input.GetKeyDown(KeyCode.Space))
        {
            // Debug.Log("[GSM] Space pressed - advancing to next");
            SwitchToNext();
        }
    }


    void SetPlayerOptions()
    {
        // Debug.Log($"[GSM] SetPlayerOptions() - playerAmmount = {playerAmmount}");

        if (teamChangeRadioButton == null)
        {
            Debug.LogWarning("[GSM] RadioButtonUI reference is missing!");
            return;
        }

        string[] options = new string[playerAmmount + 1];
        options[0] = "All";

        for (int i = 1; i <= playerAmmount; i++)
        {
            options[i] = $"Player {i}";
        }

        teamChangeRadioButton.SetOptions(options);
        // Debug.Log($"[GSM] Radio button options set: {string.Join(", ", options)}");
    }

    public int getPlayerAmmount()
    {
        return playerAmmount;
    }

    public Camera GetActiveCamera()
    {
        if (currentIndex == 0)
            return GameMasterCamera;
        
        int playerIndex = currentIndex - 1;
        if (playerIndex >= 0 && playerIndex < playerCameras.Count)
            return playerCameras[playerIndex];
        
        return GameMasterCamera; // fallback
    }

    public void setReloadFlagTrue()
    {
        reloadFlag = true;
        // Debug.Log("[GSM] reloadFlag set to TRUE");
    }

    public void SwitchToNext()
    {
        int oldIndex = currentIndex;
        currentIndex++;
        if (currentIndex > playerAmmount)
        {
            currentIndex = 0; // wrap to Game Master
            // Debug.Log($"[GSM] Wrapped around from {oldIndex} to Game Master (0)");
        }
        else
        {
            // Debug.Log($"[GSM] Switching from index {oldIndex} to {currentIndex}");
        }

        SwitchToIndex(currentIndex);

        playerReportScrollViewManager.CloseCurrentPopup();
    }

    /// <summary>
    /// Switch to an index: 0 => GameMaster, 1..N => players (player index = index-1)
    /// </summary>
    private void SwitchToIndex(int index)
    {
        // Check if we need to reload player maps before switching to a player
        if (reloadFlag && index > 0)
        {
            ReloadPlayerMaps();
            reloadFlag = false;
        }

        // Debug.Log($"[GSM] SwitchToIndex({index}) called. playerAmmount = {playerAmmount}");

        closeAllMenus();

        DisableAll();

        if (index == 0)
        {
            if (GameMasterCamera == null || GameMasterCanvas == null)
            {
                Debug.LogError("[GSM] GameMaster camera or canvas is not assigned!");
                return;
            }

            actionScrollViewManager.RemoveAllActions();

            reportScrollViewManagerGameMasterIncoming.LoadAllReportsFromPlayerReportOutputManager();

            playerReportOutputScrollViewManager.DeleteAllReportsForAllPlayer();
            playerReportOutputScrollViewManager.CloseCurrentPopup();

            GameMasterCamera.enabled = true;
            ToggleAudioListener(GameMasterCamera, true);

            GameMasterCanvas.gameObject.SetActive(true);
            GameMasterCamera.gameObject.tag = "MainCamera";

            objectPlacer?.setGroundLayerForPlacement(gameMasterGroundLayer);

            // Use CameraMover component on GameMasterCamera
            var cameraMover = GameMasterCamera.GetComponent<CameraMover>();
            if (cameraMover != null)
            {
                // Assuming null means special case, handle inside CameraMover accordingly
                cameraMover.MoveOverMap(null);
            }
            else
            {
                Debug.LogWarning("[GSM] CameraMover component missing on GameMasterCamera. Falling back to direct positioning.");
                PositionCameraOverMap(GameMasterCamera, null);
            }

            // Debug.Log($"[GSM] GameMasterCamera position after positioning: {GameMasterCamera.transform.position}");
            // Debug.Log("[GSM] Now in Game Master view.");
            // LogState();
            return;
        }

        if (index == 1)
        {
            reportScrollViewManager.DistributeReportsToPlayers();
            reportScrollViewManager.CloseCurrentPopup();
            reportScrollViewManager.DeleteAllReports();
            reportScrollViewManagerGameMasterIncoming.ClearAllEntries();
        }

        int playerListIndex = index - 1;
        if (playerListIndex < 0 || playerListIndex >= playerAmmount)
        {
            Debug.LogError($"[GSM] Invalid player index {playerListIndex} (from requested index {index}).");
            return;
        }

        Camera cam = playerCameras[playerListIndex];
        Canvas cvs = playerCanvases[playerListIndex];

        if (cam == null || cvs == null)
        {
            Debug.LogError($"[GSM] Player {index} camera or canvas is null.");
            SwitchToIndex(0);
            return;
        }

        cam.enabled = true;
        ToggleAudioListener(cam, true);

        cvs.gameObject.SetActive(true);
        cam.gameObject.tag = "MainCamera";

        objectPlacer?.setGroundLayerForPlacement(playerGroundLayer);

        if (playerMaps != null && playerListIndex < playerMaps.Count)
        {
            var cameraMover = cam.GetComponent<CameraMover>();
            if (cameraMover != null)
            {
                cameraMover.MoveOverMap(playerMaps[playerListIndex]);
            }
            else
            {
                Debug.LogWarning($"[GSM] CameraMover component missing on Player {index} camera. Falling back to direct positioning.");
                PositionCameraOverMap(cam, playerMaps[playerListIndex]);
            }

            // Debug.Log($"[GSM] Player {index} camera position after positioning: {cam.transform.position}");
        }

        // Debug.Log($"[GSM] Now in Player {index} view. (playerListIndex = {playerListIndex})");
        // LogState();
    }

    private void closeAllMenus()
    {
        Canvas player1Cvs = playerCanvases[0];
        Canvas player2Cvs = playerCanvases[1];
        Canvas player3Cvs = playerCanvases[2];
        Canvas player4Cvs = playerCanvases[3];

        GameMasterCanvas.GetComponent<UiControllerCreateSessionGameScene>().HideMenu();
        player1Cvs.GetComponent<UiControllerMainGameScene>().HideMenu();
        player2Cvs.GetComponent<UiControllerMainGameScene>().HideMenu();
        player3Cvs.GetComponent<UiControllerMainGameScene>().HideMenu();
        player4Cvs.GetComponent<UiControllerMainGameScene>().HideMenu();

    }

    private void DisableAll()
    {
        if (verboseLogs) Debug.Log("[GSM] DisableAll() - disabling all cameras & canvases");

        if (GameMasterCamera != null)
        {
            GameMasterCamera.enabled = false;
            // --- AUDIO: disable listener on this camera ---
            ToggleAudioListener(GameMasterCamera, false);

            GameMasterCamera.gameObject.tag = "Untagged";
        }
        if (GameMasterCanvas != null)
        {
            GameMasterCanvas.gameObject.SetActive(false);
        }

        if (playerCameras != null)
        {
            for (int i = 0; i < playerCameras.Count; i++)
            {
                var cam = playerCameras[i];
                if (cam != null)
                {
                    cam.enabled = false;
                    // --- AUDIO: disable listener on each player camera ---
                    ToggleAudioListener(cam, false);

                    cam.gameObject.tag = "Untagged";
                }
            }
        }

        if (playerCanvases != null)
        {
            for (int i = 0; i < playerCanvases.Count; i++)
            {
                var cvs = playerCanvases[i];
                if (cvs != null) cvs.gameObject.SetActive(false);
            }
        }
    }

    // Helper: log current important state
    private void LogState()
    {
        Debug.Log($"[GSM] LogState -> currentIndex = {currentIndex}, playerAmmount = {playerAmmount}");
        Debug.Log($"[GSM] GameMasterCamera assigned: {GameMasterCamera != null}, GameMasterCanvas assigned: {GameMasterCanvas != null}");
        Debug.Log($"[GSM] playerCameras count = {(playerCameras != null ? playerCameras.Count : 0)}, playerCanvases count = {(playerCanvases != null ? playerCanvases.Count : 0)}");

        if (playerCameras != null)
        {
            for (int i = 0; i < playerCameras.Count; i++)
            {
                Debug.Log($"[GSM] playerCameras[{i}] = {(playerCameras[i] != null ? playerCameras[i].name : "NULL")}");
            }
        }
        if (playerCanvases != null)
        {
            for (int i = 0; i < playerCanvases.Count; i++)
            {
                Debug.Log($"[GSM] playerCanvases[{i}] = {(playerCanvases[i] != null ? playerCanvases[i].name : "NULL")}");
            }
        }
    }

    // Optional: public UI hook for a "Next" button
    public void OnNextButtonPressed()
    {
        // Debug.Log("[GSM] OnNextButtonPressed() called");
        SwitchToNext();
    }

    // ---- small helper: toggle AudioListener on a camera ----
    private void ToggleAudioListener(Camera cam, bool enable)
    {
        if (cam == null) return;
        AudioListener al = cam.GetComponent<AudioListener>();
        if (al != null)
        {
            al.enabled = enable;
        }
        else
        {
            // If there's no AudioListener on the camera, nothing to toggle.
            // (This is intentional — don't auto-add components here.)
            if (verboseLogs)
                Debug.Log($"[GSM] ToggleAudioListener: Camera '{cam.name}' has no AudioListener component to {(enable ? "enable" : "disable")}.");
        }
    }
}