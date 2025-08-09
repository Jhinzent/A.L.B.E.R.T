using UnityEngine;

public class SceneTracker : MonoBehaviour
{
    public static SceneTracker Instance { get; private set; }

    public string LastSceneName { get; private set; }

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetLastScene(string sceneName)
    {
        LastSceneName = sceneName;
    }
}