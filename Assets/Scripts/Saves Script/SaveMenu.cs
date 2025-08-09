using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SaveMenuLoad : MonoBehaviour
{
    public Transform contentPanel; // The panel inside the scroll view to hold save buttons
    public Button saveButtonPrefab; // Prefab for save buttons

    private string selectedSave;

    void Start()
    {
        PopulateSaves();
    }

    private void PopulateSaves()
{
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

    LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel as RectTransform);
}

    private void OnSaveSelected(string saveName)
    {
        selectedSave = saveName;
        Debug.Log("Selected save: " + saveName);
    }

    public void OnLoadButtonClick()
{
    if (!string.IsNullOrEmpty(selectedSave))
    {
        PlayerPrefs.SetString("LoadSave", selectedSave); // Store the save name
        PlayerPrefs.SetString("CreateSave", ""); // Store the save name
        SceneManager.LoadScene("CreateSessionGameScene"); // Load the main scene
    }
    else
    {
        Debug.LogError("No save selected!");
    }
}
}