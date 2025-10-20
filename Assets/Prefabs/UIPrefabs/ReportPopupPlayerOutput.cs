using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ReportPopupPlayerOutput : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_InputField titleInputField;
    public TMP_InputField descriptionInput;
    public RadioButtonUI actionTypeRadioButton;

    public Button closeButton;
    public Button deleteButton;

    // Events
    public event Action OnDeleteRequested;
    public event Action OnCloseRequested;
    public event Action<string, string> OnDataChanged; 
    public event Action<string> OnTitleChanged;
    // Note: no team here, just description and actionType
    
    private ReportEntry reportEntry;

    // Internal state
    private string selectedActionType = "Action";

    // Properties
    public string Description
    {
        get => descriptionInput.text;
        set => descriptionInput.text = value;
    }

    public string SelectedActionType
    {
        get => selectedActionType;
        set
        {
            selectedActionType = value;
            actionTypeRadioButton.SelectOption(value);
        }
    }

    // Initialization of popup
    // Team is fixed, so no team parameter or team setup
    public void Initialize(ReportEntry entry)
    {
        reportEntry = entry;
        
        SetupTitle();
        SetupActionTypeRadioButton();

        // Load saved or default values
        Description = entry.Description;
        SelectedActionType = entry.ActionType;

        // Set up listeners
        descriptionInput.onValueChanged.RemoveAllListeners();
        descriptionInput.onValueChanged.AddListener(OnInputChanged);

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => OnCloseRequested?.Invoke());

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => OnDeleteRequested?.Invoke());
    }

    private void SetupActionTypeRadioButton()
    {
        string[] actionOptions = { "Action", "Movement", "Combat", "Investigate", "Watch", "Custom" };
        actionTypeRadioButton.SetOptions(actionOptions);

        actionTypeRadioButton.OnOptionSelected -= OnActionTypeSelected;
        actionTypeRadioButton.OnOptionSelected += OnActionTypeSelected;

        actionTypeRadioButton.SelectOption(selectedActionType);
    }

    private void OnActionTypeSelected(string selected)
    {
        selectedActionType = selected;
        reportEntry.ActionType = selected;
        OnDataChanged?.Invoke(Description, selectedActionType);
    }

    private void OnInputChanged(string _)
    {
        reportEntry.Description = Description;
        OnDataChanged?.Invoke(Description, selectedActionType);
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

    // Utility getters if needed externally
    public string GetSelectedActionType() => selectedActionType;
    public string GetDescription() => Description;
}