using UnityEngine;
using UnityEngine.SceneManagement;

public class UiControllerMainGameScene : MonoBehaviour
{
    public GameObject resumeButton;
    public GameObject settingsButton;
    public GameObject mainMenuButton;

    public GameObject uiPlane; // The main UI menu panel
    public GameObject itemScrollView;
    public GameObject drawScrollView;
    public bool isMenuActive = false;
    public static bool IsMenuActive { get; private set; }

    void Awake()
    {
        HideMenu();
        HideAllScrollViews();
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
        IsMenuActive = isMenuActive;
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

    private void HideMenu()
    {
        isMenuActive = false;
        IsMenuActive = false;
        uiPlane.SetActive(false);
        HideAllScrollViews();
        EnableMainButtons();
        Time.timeScale = 1f;
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
}