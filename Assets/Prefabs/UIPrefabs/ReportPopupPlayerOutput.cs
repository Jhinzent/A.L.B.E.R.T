using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ReportPopupPlayerOutput : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField descriptionInput;
    public RadioButtonUI actionTypeRadioButton;

    public Button closeButton;
    public Button deleteButton;

    // Events
    public event Action OnDeleteRequested;
    public event Action OnCloseRequested;
    public event Action<string, string> OnDataChanged; 
    // Note: no team here, just description and actionType

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
    public void Initialize(string description, string actionType = "Action")
    {
        SetupActionTypeRadioButton();

        // Load saved or default values
        Description = description;
        SelectedActionType = actionType;

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
        OnDataChanged?.Invoke(Description, selectedActionType);
    }

    private void OnInputChanged(string _)
    {
        OnDataChanged?.Invoke(Description, selectedActionType);
    }

    // Utility getters if needed externally
    public string GetSelectedActionType() => selectedActionType;
    public string GetDescription() => Description;
}