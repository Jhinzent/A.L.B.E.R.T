using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ReportPopupPlayer : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_InputField titleInputField;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text actionTypeText;

    [SerializeField] private Button closeButton;

    // Events
    public event Action OnCloseRequested;
    public event Action<string> OnTitleChanged;
    
    private ReportEntry reportEntry;

    /// <summary>
    /// Initializes the popup with report data. 
    /// Player popups are view-only.
    /// </summary>
    public void Initialize(ReportEntry entry)
    {
        reportEntry = entry;
        
        SetupTitle();
        
        if (descriptionText != null)
            descriptionText.text = string.IsNullOrEmpty(entry.Description) ? "(No description provided)" : entry.Description;

        if (actionTypeText != null)
            actionTypeText.text = $"{entry.ActionType}";

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => OnCloseRequested?.Invoke());
        }
    }
    
    private void SetupTitle()
    {
        if (titleText != null)
        {
            titleText.text = reportEntry.ReportName;
            
            // Make title clickable
            Button titleButton = titleText.GetComponent<Button>();
            if (titleButton == null)
                titleButton = titleText.gameObject.AddComponent<Button>();
                
            titleButton.onClick.RemoveAllListeners();
            titleButton.onClick.AddListener(StartEditingTitle);
        }
        
        if (titleInputField != null)
        {
            titleInputField.text = reportEntry.ReportName;
            titleInputField.gameObject.SetActive(false);
            titleInputField.onEndEdit.RemoveAllListeners();
            titleInputField.onEndEdit.AddListener(FinishEditingTitle);
        }
    }
    
    private void StartEditingTitle()
    {
        if (titleText != null && titleInputField != null)
        {
            titleText.gameObject.SetActive(false);
            titleInputField.gameObject.SetActive(true);
            titleInputField.text = reportEntry.ReportName;
            titleInputField.Select();
            titleInputField.ActivateInputField();
        }
    }
    
    private void FinishEditingTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            newTitle = "Untitled Report";
            
        reportEntry.ReportName = newTitle;
        
        if (titleText != null)
            titleText.text = newTitle;
            
        if (titleInputField != null)
            titleInputField.gameObject.SetActive(false);
            
        if (titleText != null)
            titleText.gameObject.SetActive(true);
            
        OnTitleChanged?.Invoke(newTitle);
    }
}