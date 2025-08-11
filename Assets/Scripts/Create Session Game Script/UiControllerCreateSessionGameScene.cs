using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UiControllerCreateSessionGameScene : MonoBehaviour
{
    public GameObject resumeButton;
    public GameObject settingsButton;
    public GameObject loadSaveButton;
    public GameObject saveButton;
    public GameObject mainMenuButton;



    public GameObject uiPlane; // The main UI menu panel
    public GameObject saveScrollView; // The scroll view for saves (initially disabled)
    public Transform contentPanel; // The panel inside the scroll view that holds save buttons
    public Button saveButtonPrefab; // Prefab for save buttons
    public Button loadSessionButton; // Button to load selected save
    public Button cancelButton; // Button to cancel loading
    public GameMasterMapLoader gameMasterMapLoader;
    public GeneralSessionManager generalSessionManager;

    public GameObject mapEditingPanel;
    public GameObject teamEditingPanel;

    public Button enableMapEditingButton;
    public Button enableTeamEditingButton;
    public static bool IsMenuActive { get; private set; }

    public GameObject itemScrollView;
    public GameObject terrainScrollView;
    public GameObject unitScrollView;

    private bool isMenuActive = false;
    private string selectedSave;

    void Awake()
    {
        HideMenu();
        mapEditingPanel.SetActive(false);
        teamEditingPanel.SetActive(false);
        loadSessionButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);
        // Initially hide all scroll views
        HideAllScrollViews();
    }

    public void ToggleMenuButtonPress()
    {
        ToggleMenu();
    }

    private void HideAllScrollViews()
    {
        itemScrollView.SetActive(false);
        terrainScrollView.SetActive(false);
        unitScrollView.SetActive(false);
    }

    public void ToggleMapEditingPanel()
    {
        bool isCurrentlyActive = mapEditingPanel.activeSelf;
        mapEditingPanel.SetActive(!isCurrentlyActive);
        teamEditingPanel.SetActive(false);
    }

    public void ToggleTeamEditingPanel()
    {
        bool isCurrentlyActive = teamEditingPanel.activeSelf;
        teamEditingPanel.SetActive(!isCurrentlyActive);
        mapEditingPanel.SetActive(false);
    }


    public void toggleTerrainScrollView()
    {
        bool shouldShow = !terrainScrollView.activeSelf;
        HideAllScrollViews();
        terrainScrollView.SetActive(shouldShow);
    }

    public void toggleItemScrollView()
    {
        bool shouldShow = !itemScrollView.activeSelf;
        HideAllScrollViews();
        itemScrollView.SetActive(shouldShow);
    }

    public void toggleUnitScrollView() // ✅ NEW: toggle for unit scroll view
    {
        bool shouldShow = !unitScrollView.activeSelf;
        HideAllScrollViews();
        unitScrollView.SetActive(shouldShow);
    }

    void Update()
    {
        // Toggle menu visibility with Esc key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (saveScrollView.activeSelf)
            {
                CancelLoad(); // Close save menu properly
            }
            else
            {
                ToggleMenu();
            }
        }
    }

    public void hitResumeButton()
    {
        HideMenu();
    }

    public void hitSettingsButton()
    {
        Debug.Log("Settings Open...");
    }

    public void hitLoadSaveButton()
    {
        ShowSaveMenu();
    }

    public void hitSaveButton()
    {
        string currentLoadedSave = generalSessionManager.getLoadSaveName();
        string currentCreatedSave = generalSessionManager.getCreateSaveName();
        string currentGameSessionSave = generalSessionManager.getGameSessionSaveName();

        if (!string.IsNullOrEmpty(currentLoadedSave))
        {
            SaveSystem.SaveSession(currentLoadedSave);
            Debug.Log($"Session saved as {currentLoadedSave}");
        }

        if (!string.IsNullOrEmpty(currentCreatedSave))
        {
            SaveSystem.SaveSession(currentCreatedSave);
            Debug.Log($"Session saved as {currentCreatedSave}");
        }

        if (!string.IsNullOrEmpty(currentGameSessionSave))
        {
            SaveSystem.SaveSession(currentGameSessionSave);
            Debug.Log($"Session saved as {currentGameSessionSave}");
        }
    }

    public void hitMainMenuButton()
    {
        SceneManager.LoadScene("MainMenuScene");
        IsMenuActive = false;
    }

    private void ToggleMenu()
    {
        isMenuActive = !isMenuActive;
        IsMenuActive = isMenuActive;
        uiPlane.SetActive(isMenuActive);
        saveScrollView.SetActive(false); // Ensure save menu is hidden initially

        if (isMenuActive)
        {
            EnableMainButtons();
        }
        else
        {
            HideMenu();
        }
    }

    private void HideMenu()
    {
        isMenuActive = false;
        IsMenuActive = false;
        uiPlane.SetActive(false);
        saveScrollView.SetActive(false);
        HideAllScrollViews();
        EnableMainButtons();
        Time.timeScale = 1f;
    }

    private void ShowSaveMenu()
    {
        saveScrollView.SetActive(true); // Show the save menu
        DisableMainButtons(); // Hide main menu buttons

        // Make sure the load and cancel buttons are visible
        loadSessionButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(true);

        // Set interactivity correctly
        loadSessionButton.interactable = false; // Initially disabled until a save is selected
        cancelButton.interactable = true;

        PopulateSaves(); // Populate the list of saves
    }
    private void PopulateSaves()
    {
        // Clear previous buttons to prevent duplication
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        List<string> saves = SaveSystem.GetAllSaves(); // Get all saves
        foreach (string save in saves)
        {
            Button button = Instantiate(saveButtonPrefab, contentPanel); // Instantiate button
            button.transform.SetParent(contentPanel, false); // Ensure correct parent without scaling issues

            TextMeshProUGUI textComponent = button.GetComponentInChildren<TextMeshProUGUI>(); // Get text component
            if (textComponent != null)
            {
                textComponent.text = save; // Set button text
            }
            else
            {
                Debug.LogError("TextMeshProUGUI component not found in saveButtonPrefab!");
            }

            button.onClick.AddListener(() => OnSaveSelected(save)); // Add click listener
        }

        // Ensure buttons are correctly updated
        loadSessionButton.interactable = false; // Disabled until a save is selected
        loadSessionButton.onClick.RemoveAllListeners();
        loadSessionButton.onClick.AddListener(() => LoadSelectedSave());

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => CancelLoad());

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel as RectTransform);
    }

    private void OnSaveSelected(string saveName)
    {
        selectedSave = saveName;
        Debug.Log("Selected save: " + saveName);
        loadSessionButton.interactable = true; // Enable load button
    }

    private void LoadSelectedSave()
    {
        if (!string.IsNullOrEmpty(selectedSave))
        {
            PlayerPrefs.SetString("LoadSave", selectedSave); // Store the save name
            SceneManager.LoadScene("CreateSessionGameScene"); // Load the main scene
        }
        else
        {
            Debug.LogError("No save selected!");
        }
    }

    private void CancelLoad()
    {
        saveScrollView.SetActive(false); // Hide the save menu

        // Hide load and cancel buttons again
        loadSessionButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);

        EnableMainButtons(); // Show the main menu buttons again
    }


    private void DisableMainButtons()
    {
        resumeButton.SetActive(false);
        settingsButton.SetActive(false);
        loadSaveButton.SetActive(false);
        saveButton.SetActive(false);
        mainMenuButton.SetActive(false);
    }

    private void EnableMainButtons()
    {
        resumeButton.SetActive(true);
        settingsButton.SetActive(true);
        loadSaveButton.SetActive(true);
        saveButton.SetActive(true);
        mainMenuButton.SetActive(true);
    }
}
