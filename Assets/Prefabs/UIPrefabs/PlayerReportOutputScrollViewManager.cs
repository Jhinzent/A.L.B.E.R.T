using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerReportOutputScrollViewManager : MonoBehaviour
{
    [Header("Shared Prefabs & UI")]
    public GameObject reportItemPrefab;
    public GameObject createReportButtonPrefab;
    public GameObject reportPopupPrefab;

    [Header("Player Popup Parents (one per player, for their camera)")]
    public List<Transform> popupParents = new List<Transform>();

    [Header("Player ScrollViews (fill all 4 slots in inspector)")]
    public List<RectTransform> playerContentPanels = new List<RectTransform>(4);

    private List<List<ReportEntry>> playerReportEntries = new List<List<ReportEntry>>();
    private List<GameObject> createButtonInstances = new List<GameObject>();
    private static GameObject currentlyOpenPopup;
    private static ReportEntry currentlyOpenEntry;
    private List<int> nextReportNumberPerPlayer = new List<int>();

    private readonly string[] playerNames = { "Player 1", "Player 2", "Player 3", "Player 4" };

    private void Start()
    {
        for (int i = 0; i < playerContentPanels.Count; i++)
        {
            playerReportEntries.Add(new List<ReportEntry>());
            nextReportNumberPerPlayer.Add(1);  // Initialize report numbering for each player
            CreateAddButton(i);
        }
    }

    private void CreateAddButton(int playerIndex)
    {
        if (playerContentPanels[playerIndex] == null) return;

        GameObject btnObj = Instantiate(createReportButtonPrefab, playerContentPanels[playerIndex]);
        Button btn = btnObj.GetComponent<Button>();

        int indexCopy = playerIndex; // capture for closure
        btn.onClick.AddListener(() => CreateNewReportForPlayer(indexCopy));

        createButtonInstances.Add(btnObj);
    }

    public void CreateNewReportForPlayer(int playerIndex)
    {
        var reports = playerReportEntries[playerIndex];
        int newReportNumber = nextReportNumberPerPlayer[playerIndex];  // Use the next available number

        ReportEntry entry = new ReportEntry
        {
            ReportName = $"Report {newReportNumber}",
            Team = playerNames[playerIndex],
            ActionType = "Action",
            Description = ""
        };

        GameObject entryObj = Instantiate(reportItemPrefab, playerContentPanels[playerIndex]);
        int insertIndex = Mathf.Max(0, playerContentPanels[playerIndex].childCount - 1);
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
        reports.Add(entry);

        // Increment the next report number for this player
        nextReportNumberPerPlayer[playerIndex]++;

        // Keep "+" button last
        createButtonInstances[playerIndex].transform.SetAsLastSibling();

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

        int playerIndex = -1;
        for (int i = 0; i < playerNames.Length; i++)
        {
            if (playerNames[i] == entry.Team)
            {
                playerIndex = i;
                break;
            }
        }

        if (playerIndex < 0 || playerIndex >= popupParents.Count || popupParents[playerIndex] == null)
        {
            Debug.LogWarning($"No valid popup parent found for team '{entry.Team}'. Popup not created.");
            return;
        }

        GameObject popupInstance = Instantiate(reportPopupPrefab, popupParents[playerIndex]);
        currentlyOpenPopup = popupInstance;
        currentlyOpenEntry = entry;

        var popupScript = popupInstance.GetComponent<ReportPopupPlayerOutput>();
        popupScript.Initialize(entry);

        popupScript.OnDataChanged += (desc, actionType) =>
        {
            if (entry.DisplayObject != null)
            {
                TMP_Text text = entry.DisplayObject.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.text = entry.ReportName;
                    text.color = GetTeamColor(entry.Team);
                }
            }
        };
        
        popupScript.OnTitleChanged += (newTitle) =>
        {
            if (entry.DisplayObject != null)
            {
                TMP_Text text = entry.DisplayObject.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.text = newTitle;
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
            RemoveEntryFromPlayerLists(entry);
            CloseCurrentPopup();
        };
    }

    private void RemoveEntryFromPlayerLists(ReportEntry entry)
    {
        foreach (var reports in playerReportEntries)
        {
            if (reports.Remove(entry))
                break;
        }
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

    public void ReceiveReports(List<ReportEntry> reportsFromMain)
    {
        // Reset all lists and counters before distributing
        for (int i = 0; i < playerReportEntries.Count; i++)
        {
            playerReportEntries[i].Clear();
            nextReportNumberPerPlayer[i] = 1;  // reset
        }

        foreach (var entry in reportsFromMain)
        {
            int playerIndex = Array.IndexOf(playerNames, entry.Team);
            if (playerIndex >= 0 && playerIndex < playerContentPanels.Count && playerContentPanels[playerIndex] != null)
            {
                AddReportToPlayer(entry, playerIndex);

                // Update nextReportNumberPerPlayer to max existing report number + 1
                int reportNum = ParseReportNumber(entry.ReportName);
                if (reportNum >= nextReportNumberPerPlayer[playerIndex])
                {
                    nextReportNumberPerPlayer[playerIndex] = reportNum + 1;
                }
            }
            else
            {
                Debug.LogWarning($"Unknown team '{entry.Team}' or missing player panel.");
            }
        }
    }

    private int ParseReportNumber(string reportName)
    {
        // Assumes report name format is "Report X"
        var parts = reportName.Split(' ');
        if (parts.Length == 2 && int.TryParse(parts[1], out int num))
        {
            return num;
        }
        return 0; // fallback
    }

    private void AddReportToPlayer(ReportEntry entry, int playerIndex)
    {
        var reports = playerReportEntries[playerIndex];
        reports.Add(entry);

        GameObject entryObj = Instantiate(reportItemPrefab, playerContentPanels[playerIndex]);
        int insertIndex = Mathf.Max(0, playerContentPanels[playerIndex].childCount - 1);
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

        // Keep "+" button last
        createButtonInstances[playerIndex].transform.SetAsLastSibling();
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
    /// Delete all reports from a specific player scrollview.
    /// </summary>
    public void DeleteAllReportsForPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerReportEntries.Count) return;

        CloseCurrentPopup();

        var reports = playerReportEntries[playerIndex];
        foreach (var entry in reports)
        {
            if (entry.DisplayObject != null)
                Destroy(entry.DisplayObject);
        }
        reports.Clear();
    }

    public void DeleteAllReportsForAllPlayer()
    {
        DeleteAllReportsForPlayer(0);
        DeleteAllReportsForPlayer(1);
        DeleteAllReportsForPlayer(2);
        DeleteAllReportsForPlayer(3);
    }

    /// <summary>
    /// Gets a combined list of all current ReportEntry objects from all players (output only).
    /// </summary>
    public List<ReportEntry> GetAllReports()
    {
        List<ReportEntry> allReports = new List<ReportEntry>();
        foreach (var reports in playerReportEntries)
        {
            allReports.AddRange(reports);
        }
        return allReports;
    }
}