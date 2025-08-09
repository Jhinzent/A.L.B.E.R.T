using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionManager : MonoBehaviour
{
    public void LoadSave()
    {
        // For now, just load the GameScene directly
        SceneTracker.Instance.SetLastScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("EditMapMenuScene");
    }

    public void CreateSave()
    {
        // For now, just load the GameScene directly
        SceneTracker.Instance.SetLastScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("CreateMapMenuScene");
    }

    public void StartSession()
    {
        // For now, just load the GameScene directly
        SceneTracker.Instance.SetLastScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("StartSessionMenuScene");
    }
}