using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ReportPopupGameMasterIncoming : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text teamText;
    [SerializeField] private TMP_Text actionTypeText;

    [SerializeField] private Button closeButton;

    // Events
    public event Action OnCloseRequested;

    /// <summary>
    /// Initializes the popup with report data. 
    /// Player popups are view-only.
    /// </summary>
    public void Initialize(string description, string team = "Player 1", string actionType = "Action")
    {
        if (descriptionText != null)
            descriptionText.text = string.IsNullOrEmpty(description) ? "(No description provided)" : description;

        if (teamText != null)
            teamText.text = $"Team: {team}";

        if (actionTypeText != null)
            actionTypeText.text = $"Action: {actionType}";

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => OnCloseRequested?.Invoke());
        }
    }
}