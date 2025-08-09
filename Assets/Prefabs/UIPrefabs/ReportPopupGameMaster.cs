using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ReportPopupGameMaster : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField descriptionInput;
    public RadioButtonUI teamRadioButton;
    public RadioButtonUI actionTypeRadioButton;

    public Button closeButton;
    public Button deleteButton;

    // Events
    public event Action OnDeleteRequested;
    public event Action OnCloseRequested;
    public event Action<string, string, string> OnDataChanged;

    // Internal state
    private string selectedTeam = "Player 1";
    private string selectedActionType = "Action";

    // Properties
    public string Description
    {
        get => descriptionInput.text;
        set => descriptionInput.text = value;
    }

    public string SelectedTeam
    {
        get => selectedTeam;
        set
        {
            selectedTeam = value;
            teamRadioButton.SelectOption(value);
        }
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
    public void Initialize(string description, string team = "Player 1", string actionType = "Action")
    {
        SetupRadioButtons();

        // Load saved or default values
        Description = description;
        SelectedTeam = team;
        SelectedActionType = actionType;

        // Set up listeners
        descriptionInput.onValueChanged.RemoveAllListeners();
        descriptionInput.onValueChanged.AddListener(OnInputChanged);

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => OnCloseRequested?.Invoke());

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => OnDeleteRequested?.Invoke());
    }

    private void SetupRadioButtons()
    {
        // Team Radio Button
        string[] teamOptions = { "Player 1", "Player 2", "Player 3", "Player 4" };
        teamRadioButton.SetOptions(teamOptions);

        teamRadioButton.OnOptionSelected -= OnTeamSelected;
        teamRadioButton.OnOptionSelected += OnTeamSelected;

        teamRadioButton.SelectOption(selectedTeam);

        // Action Type Radio Button
        string[] actionOptions = { "Action", "Movement", "Combat", "Investigate", "Watch", "Custom" };
        actionTypeRadioButton.SetOptions(actionOptions);

        actionTypeRadioButton.OnOptionSelected -= OnActionTypeSelected;
        actionTypeRadioButton.OnOptionSelected += OnActionTypeSelected;

        actionTypeRadioButton.SelectOption(selectedActionType);
    }

    private void OnTeamSelected(string selected)
    {
        selectedTeam = selected;
        OnDataChanged?.Invoke(Description, selectedTeam, selectedActionType);
    }

    private void OnActionTypeSelected(string selected)
    {
        selectedActionType = selected;
        OnDataChanged?.Invoke(Description, selectedTeam, selectedActionType);
    }

    private void OnInputChanged(string _)
    {
        OnDataChanged?.Invoke(Description, selectedTeam, selectedActionType);
    }

    // Utility getters if needed externally
    public string GetSelectedTeam() => selectedTeam;
    public string GetSelectedActionType() => selectedActionType;
    public string GetDescription() => Description;
}