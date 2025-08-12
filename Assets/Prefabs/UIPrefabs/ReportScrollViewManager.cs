using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReportScrollViewManager : MonoBehaviour
{
    [Header("Main ScrollView UI References")]
    [SerializeField] private RectTransform contentPanel; // Main scrollview's content
    [SerializeField] private GameObject reportItemPrefab;
    [SerializeField] private GameObject reportPopupPrefab;
    [SerializeField] private GameObject createReportButtonPrefab;
    [SerializeField] private Transform popupParent;

    [Header("Extra Player ScrollViews")]
    [Tooltip("Order matters! Index 0 -> Player 1, Index 1 -> Player 2, etc.")]
    public List<RectTransform> playerScrollViewContents = new();

    [SerializeField] private PlayerReportScrollViewManager playerManager; // Reference to Player Manager

    private readonly List<ReportEntry> reportEntries = new(); // Main list
    private static GameObject currentlyOpenPopup;
    private static ReportEntry currentlyOpenEntry;

    private int reportCounter = 0;
    private GameObject createButtonInstance;

    private void Start()
    {
        // Create the "+" button in the main scrollview
        createButtonInstance = Instantiate(createReportButtonPrefab, contentPanel);
        Button createButton = createButtonInstance.GetComponent<Button>();
        if (createButton != null)
        {
            createButton.onClick.AddListener(CreateNewReport);
        }
        createButtonInstance.transform.SetAsLastSibling();
    }

    private void CreateNewReport()
{
    reportCounter++;

    ReportEntry entry = new ReportEntry
    {
        ReportName = $"Report {reportCounter}",
        Team = "Player 1",
        ActionType = "Action",
        Description = ""
    };

    GameObject entryObj = Instantiate(reportItemPrefab, contentPanel);
    int insertIndex = Mathf.Max(0, contentPanel.childCount - 1); // Before the create button
    entryObj.transform.SetSiblingIndex(insertIndex);

    TMP_Text text = entryObj.GetComponentInChildren<TMP_Text>();
    if (text != null)
    {
        text.text = entry.ReportName;
        text.color = GetTeamColor(entry.Team);
    }

    Button button = entryObj.GetComponent<Button>();
    if (button != null)
    {
        button.onClick.AddListener(() => OnReportClicked(entry));
    }

    entry.DisplayObject = entryObj;
    reportEntries.Insert(0, entry);

    // **Make sure the create button is always last in the hierarchy**
    if (createButtonInstance != null)
    {
        createButtonInstance.transform.SetAsLastSibling();
    }

    OpenPopupForEntry(entry);
}

    private void OnReportClicked(ReportEntry entry)
    {
        if (currentlyOpenEntry == entry)
        {
            CloseCurrentPopup();
        }
        else
        {
            OpenPopupForEntry(entry);
        }
    }

    private void OpenPopupForEntry(ReportEntry entry)
    {
        CloseCurrentPopup();

        GameObject popupInstance = Instantiate(reportPopupPrefab, popupParent);
        currentlyOpenPopup = popupInstance;
        currentlyOpenEntry = entry;

        var popupScript = popupInstance.GetComponent<ReportPopupGameMaster>();
        popupScript.Initialize(entry.Description, entry.Team, entry.ActionType);

        popupScript.OnDataChanged += (desc, team, actionType) =>
        {
            entry.Description = desc;
            entry.Team = team;
            entry.ActionType = actionType;

            if (entry.DisplayObject != null)
            {
                TMP_Text text = entry.DisplayObject.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.text = entry.ReportName;
                    text.color = GetTeamColor(team);
                }
            }
        };

        popupScript.OnCloseRequested += CloseCurrentPopup;
        popupScript.OnDeleteRequested += () =>
        {
            Debug.Log("[ReportScrollViewManager] Deleting report: " + entry.ReportName);
            if (entry.DisplayObject != null)
            {
                Destroy(entry.DisplayObject);
            }
            reportEntries.Remove(entry);
            CloseCurrentPopup();
        };
    }

    public void CloseCurrentPopup()
    {
        if (currentlyOpenPopup != null)
        {
            Destroy(currentlyOpenPopup);
            currentlyOpenPopup = null;
            currentlyOpenEntry = null;
        }
    }

    private Color GetTeamColor(string team)
    {
        return team switch
        {
            "Player 1" => Color.red,
            "Player 2" => Color.blue,
            "Player 3" => Color.green,
            "Player 4" => Color.yellow,
            _ => Color.gray,
        };
    }

    /// <summary>
    /// Calls the Player Manager to fill the player scrollviews with the current report list,
    /// then clears the main list and optionally destroys the create button.
    /// </summary>
    public void DistributeReportsToPlayers()
    {
        if (playerManager != null)
        {
            Debug.Log("[ReportScrollViewManager] Distributing reports to Player Manager: " + reportEntries);
            playerManager.ReceiveReports(reportEntries);
        }
        else
        {
            Debug.LogWarning("[ReportScrollViewManager] Player Manager reference is missing!");
        }

        // Clear the main list and destroy the Create button instance (optional)
        reportEntries.Clear();

        if (createButtonInstance != null)
        {
            Destroy(createButtonInstance);
            createButtonInstance = null;
        }
    }

    /// <summary>
    /// Deletes all reports currently in the main list and removes their UI elements.
    /// </summary>
    public void DeleteAllReports()
{
    CloseCurrentPopup();

    // Remove all children from the content panel except the create button
    foreach (Transform child in contentPanel)
    {
        if (child.gameObject != createButtonInstance)
        {
            Destroy(child.gameObject);
        }
    }

    // Clear the internal list and reset the counter
    reportEntries.Clear();
    reportCounter = 0;

    // Recreate the "+" button if it no longer exists
    if (createButtonInstance == null && createReportButtonPrefab != null)
    {
        createButtonInstance = Instantiate(createReportButtonPrefab, contentPanel);
        Button createButton = createButtonInstance.GetComponent<Button>();
        if (createButton != null)
        {
            createButton.onClick.AddListener(CreateNewReport);
        }
    }

    // Ensure the create button is last in the hierarchy
    createButtonInstance.transform.SetAsLastSibling();

    Debug.Log("[ReportScrollViewManager] All report objects removed, create button ensured.");
}
}