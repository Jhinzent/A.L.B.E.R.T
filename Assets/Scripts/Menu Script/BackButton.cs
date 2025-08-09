using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    public void GoBack()
    {
        if (SceneTracker.Instance != null && 
            !string.IsNullOrEmpty(SceneTracker.Instance.LastSceneName))
        {
            SceneManager.LoadScene(SceneTracker.Instance.LastSceneName);
        }
        else
        {
            Debug.LogWarning("No previous scene stored or SceneTracker missing!");
        }
    }
}