using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReportScrollViewManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private GameObject reportItemPrefab;
    [SerializeField] private GameObject reportPopupPrefab;
    [SerializeField] private GameObject createReportButtonPrefab;
    [SerializeField] private Transform popupParent;

    private readonly List<ReportEntry> reportEntries = new();
    private static GameObject currentlyOpenPopup;
    private static ReportEntry currentlyOpenEntry;

    private int reportCounter = 0;
    private GameObject createButtonInstance;

    private void Start()
    {
        // Create the grayed-out "+" button at the bottom
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
            if (entry.DisplayObject != null)
            {
                Destroy(entry.DisplayObject);
            }
            reportEntries.Remove(entry);
            CloseCurrentPopup();
        };
    }

    private void CloseCurrentPopup()
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

    [System.Serializable]
    public class ReportEntry
    {
        public string ReportName;
        public string Team;
        public string ActionType;
        public string Description;
        public GameObject DisplayObject;
    }
}
