using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UiControllerMainGameScene : MonoBehaviour
{
    public GameObject resumeButton;
    public GameObject settingsButton;
    public GameObject mainMenuButton;

    public GameObject uiPlane; // The main UI menu panel
    public GameObject itemScrollView;
    public GameObject drawScrollView;
    public GameObject nextPlayerMenu;
    public TMP_InputField passphraseInputField;
    public TextMeshProUGUI timeDisplayText;
    public GeneralSessionManager generalSessionManager;
    public bool isMenuActive = false;
    public bool isNextPlayerMenuActive = false;
    public static bool IsMenuActive { get; private set; }

    void Awake()
    {
        HideMenu();
        HideAllScrollViews();
        HideNextPlayerMenu();
    }

    public void ToggleMenuButtonPress()
    {
        ToggleMenu();
    }

    private void HideAllScrollViews()
    {
        itemScrollView.SetActive(false);
        drawScrollView.SetActive(false);
    }

    public void DrawTerrainScrollView()
    {
        // If itemScrollView is open, close it before opening drawScrollView
        if (itemScrollView.activeSelf)
        {
            itemScrollView.SetActive(false);
            drawScrollView.SetActive(true);
        }
        else
        {
            // Toggle drawScrollView
            drawScrollView.SetActive(!drawScrollView.activeSelf);
        }
    }

    public void ToggleItemScrollView()
    {
        // If drawScrollView is open, close it before opening itemScrollView
        if (drawScrollView.activeSelf)
        {
            drawScrollView.SetActive(false);
            itemScrollView.SetActive(true);
        }
        else
        {
            // Toggle itemScrollView
            itemScrollView.SetActive(!itemScrollView.activeSelf);
        }
    }

    void Update()
    {
        // Toggle menu visibility with Esc key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void HitResumeButton()
    {
        HideMenu();
    }

    public void HitSettingsButton()
    {
        Debug.Log("Settings Open...");
    }

    public void HitMainMenuButton()
    {
        SceneManager.LoadScene("MainMenuScene");
        IsMenuActive = false;
    }

    private void ToggleMenu()
    {
        isMenuActive = !isMenuActive;
        IsMenuActive = isMenuActive || isNextPlayerMenuActive;
        uiPlane.SetActive(isMenuActive);

        if (isMenuActive)
        {
            EnableMainButtons();
        }
        else
        {
            HideMenu();
        }
    }

    public void HideMenu()
    {
        isMenuActive = false;
        IsMenuActive = isNextPlayerMenuActive;
        uiPlane.SetActive(false);
        HideAllScrollViews();
        EnableMainButtons();
    }

    public void ShowNextPlayerMenu()
    {
        isNextPlayerMenuActive = true;
        IsMenuActive = true;
        nextPlayerMenu.SetActive(true);
    }

    public void HideNextPlayerMenu()
    {
        isNextPlayerMenuActive = false;
        IsMenuActive = isMenuActive;
        nextPlayerMenu.SetActive(false);
        passphraseInputField.text = "";
    }

    public void OnPassphraseSubmit()
    {
        string result = generalSessionManager.ValidatePassphrase(passphraseInputField.text);

        if (result == "Successful")
        {
            passphraseInputField.text = "";
            HideNextPlayerMenu();
        }
        else
        {
            passphraseInputField.text = "Incorrect passphrase";
        }
    }

    private void DisableMainButtons()
    {
        resumeButton.SetActive(false);
        settingsButton.SetActive(false);
        mainMenuButton.SetActive(false);
    }

    private void EnableMainButtons()
    {
        resumeButton.SetActive(true);
        settingsButton.SetActive(true);
        mainMenuButton.SetActive(true);
    }

    public void UpdateTimeDisplay(string time)
    {
        if (timeDisplayText != null)
        {
            timeDisplayText.text = time;
        }
    }
}