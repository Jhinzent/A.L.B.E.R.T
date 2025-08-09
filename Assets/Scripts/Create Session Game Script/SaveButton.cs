using UnityEngine;
using TMPro; // Import the TextMeshPro namespace

public class SaveButton : MonoBehaviour
{
    public TMP_InputField saveNameInput; // Reference to the TextMeshPro input field

    public void OnSaveButtonClick()
    {
        string saveName = saveNameInput.text;
        if (!string.IsNullOrEmpty(saveName))
        {
            SaveSystem.SaveSession(saveName);
            Debug.Log("Session saved with name: " + saveName);
        }
        else
        {
            Debug.LogError("Save name is empty!");
        }
    }
}