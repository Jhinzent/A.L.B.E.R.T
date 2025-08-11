using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SaveMenu : MonoBehaviour
{
    public Transform contentPanel; // The panel inside the scroll view to hold save buttons
    public Button saveButtonPrefab; // Prefab for save buttons
    public TMP_InputField playerAmmountInputField;

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
        // Debug.Log("Selected save: " + saveName);
    }

    public void OnLoadButtonClick()
    {
        if (!string.IsNullOrEmpty(selectedSave))
        {
            PlayerPrefs.SetString("EditSave", selectedSave); // Store the save name
            SceneManager.LoadScene("EditMapScene"); // Load the main scene
        }
        else
        {
            Debug.LogError("No save selected!");
        }
    }

    public void OnStartSessionButtonClick()
    {
        if (!string.IsNullOrEmpty(selectedSave))
        {
            PlayerPrefs.SetString("StartSession", selectedSave);
            PlayerPrefs.SetString("EditSave", "");
            PlayerPrefs.SetString("CreateSave", "");
            int playerAmmount = int.Parse(playerAmmountInputField.text);
            if (playerAmmount > 0)
            {
                SceneData.playerAmmount = playerAmmount;
            }
            else SceneData.playerAmmount = 1;
            
            SceneManager.LoadScene("GameSessionScene"); // Load the main scene
        }
        else
        {
            Debug.Log("No save selected!");
        }
    }
}