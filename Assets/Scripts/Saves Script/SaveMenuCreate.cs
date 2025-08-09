using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SaveMenuCreate : MonoBehaviour
{

    private string createdSaveName;
    public GameObject inputFieldObject;

    void Start ()
    {
        createdSaveName = "";

        var input = inputFieldObject.GetComponent<TMP_InputField>();
        var se = new TMP_InputField.SubmitEvent();
        se.AddListener(ChangeSaveName);
        input.onEndEdit = se;

        //or simply use the line below, 
        //input.onEndEdit.AddListener(SubmitName);  // This also works
    }

    private void ChangeSaveName(string arg0) {
        createdSaveName = arg0;
        Debug.Log(createdSaveName);
    }

    public void OnCreateSaveButtonClick()
{
    if (!string.IsNullOrEmpty(createdSaveName))
    {
        PlayerPrefs.SetString("CreateSave", createdSaveName); // Store the save name
        PlayerPrefs.SetString("EditSave", ""); // Store the save name
        SceneManager.LoadScene("EditMapScene"); // Load the main scene
    }
    else
    {
        Debug.LogError("No save selected!");
    }
}
}