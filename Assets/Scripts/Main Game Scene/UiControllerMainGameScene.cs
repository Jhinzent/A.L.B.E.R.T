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

    public void drawTerrainScrollView()
    {
        bool wasActive = drawScrollView.activeSelf;
        HideAllScrollViews();  // Hide both scroll views first
        // Toggle the drawScrollView only if it was not active before
        drawScrollView.SetActive(!wasActive);
    }

    public void toggleItemScrollView()
    {
        bool wasActive = itemScrollView.activeSelf;
        HideAllScrollViews();  // Hide both scroll views first
        // Toggle the itemScrollView only if it was not active before
        itemScrollView.SetActive(!wasActive);
    }

    void Update()
    {
        // Toggle menu visibility with Esc key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
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