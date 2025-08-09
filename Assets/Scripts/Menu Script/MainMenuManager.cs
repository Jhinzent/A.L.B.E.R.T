using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneTracker.Instance.SetLastScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("SessionMenuScene");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
