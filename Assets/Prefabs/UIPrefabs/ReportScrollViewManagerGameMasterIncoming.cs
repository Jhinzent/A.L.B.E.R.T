using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReportScrollViewManagerGameMasterIncoming : MonoBehaviour
{
    public static List<ReportScrollViewManagerGameMasterIncoming> AllScrollViews = new();

    [Header("UI References")]
    [SerializeField] private RectTransform contentPanelUnseen; // A panel
    [SerializeField] private RectTransform viewportUnseen;

    [SerializeField] private RectTransform contentPanelSeen;   // B panel
    [SerializeField] private RectTransform viewportSeen;

    [SerializeField] private PlayerReportOutputScrollViewManager playerReportOutputScrollViewManager;

    public Transform ContentPanelA => contentPanelUnseen;
    public Transform ContentPanelB => contentPanelSeen;

    public RectTransform ViewportA => viewportUnseen;
    public RectTransform ViewportB => viewportSeen;

    [SerializeField] private GameObject reportButtonPrefab;
    [SerializeField] private GameObject reportPopupPrefab;
    [SerializeField] private Transform popupParent;

    private List<ReportEntry> reportEntriesUnseen = new();
    private List<ReportEntry> reportEntriesSeen = new();

    private static GameObject currentlyOpenPopup;
    private static ReportEntry currentlyOpenEntry;

    private void Awake()
    {
        AllScrollViews.Add(this);
    }

    private void OnDestroy()
    {
        AllScrollViews.Remove(this);
        CloseCurrentPopup();
    }

    public void LoadAllReportsFromPlayerReportOutputManager()
    {
        ClearAllEntries();

        List<ReportEntry> allReports = playerReportOutputScrollViewManager.GetAllReports();

        foreach (var report in allReports)
        {
            AddReport(report, toUnseen: true);
        }
    }

    public void AddReport(ReportEntry report, bool toUnseen)
    {
        Transform targetContent = toUnseen ? contentPanelUnseen : contentPanelSeen;
        var targetList = toUnseen ? reportEntriesUnseen : reportEntriesSeen;

        // Prevent duplicates: check if report already added
        if (targetList.Contains(report))
        {
            Debug.LogWarning("Report already added to the list!");
            return;
        }

        GameObject entryObj = Instantiate(reportButtonPrefab, targetContent);

        TMP_Text text = entryObj.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = $"{report.ReportName}";
            text.color = GetPlayerColor(report.Team);
        }

        Button button = entryObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                if (currentlyOpenEntry == report)
                {
                    CloseCurrentPopup();
                }
                else
                {
                    TryOpenReportPopup(report);
                }
            });
        }

        DraggableReport draggable = entryObj.GetComponent<DraggableReport>();
        if (draggable != null)
        {
            draggable.OriginManager = this;
            draggable.ReportData = report;
        }
        else
        {
            Debug.LogWarning("DraggableReport component missing on reportButtonPrefab!");
        }

        // Assign display object and add to list
        if (report.DisplayObject != null)
        {
            // Defensive: Destroy old displayObject if any
            Destroy(report.DisplayObject);
        }
        report.DisplayObject = entryObj;

        targetList.Add(report);

        // Subscribe safely to avoid multiple subscriptions
        report.OnDestroyed -= HandleReportDestroyed;
        report.OnDestroyed += HandleReportDestroyed;
    }

    private void TryOpenReportPopup(ReportEntry report)
    {
        CloseCurrentPopup();

        GameObject popupInstance = Instantiate(reportPopupPrefab, popupParent);
        currentlyOpenPopup = popupInstance;
        currentlyOpenEntry = report;

        RectTransform rectTransform = popupInstance.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
        }

        var popupScript = popupInstance.GetComponent<ReportPopupGameMasterIncoming>();
        if (popupScript != null)
        {
            // Initialize with report data (read-only)
            popupScript.Initialize(
                report.Description,
                report.Team,
                report.ActionType
            );

            // Only subscribe to close event
            popupScript.OnCloseRequested += CloseCurrentPopup;
        }
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

    public void RemoveReport(ReportEntry report)
    {
        bool removed = RemoveReportFromList(report, reportEntriesUnseen, contentPanelUnseen)
                    || RemoveReportFromList(report, reportEntriesSeen, contentPanelSeen);

        if (removed)
        {
            report.OnDestroyed -= HandleReportDestroyed;

            if (report.DisplayObject != null)
            {
                Destroy(report.DisplayObject);
                report.DisplayObject = null;
            }

            // If the removed report was the currently open one, close popup
            if (currentlyOpenEntry == report)
            {
                CloseCurrentPopup();
            }
        }
    }

    private bool RemoveReportFromList(ReportEntry report, List<ReportEntry> list, Transform parent)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] == report)
            {
                if (report.DisplayObject != null)
                    Destroy(report.DisplayObject);

                list.RemoveAt(i);
                report.DisplayObject = null;
                return true;
            }
        }
        return false;
    }

    public void RemoveReportsForTeam(string team)
    {
        RemoveReportsForTeamFromList(team, reportEntriesUnseen);
        RemoveReportsForTeamFromList(team, reportEntriesSeen);
    }

    private void RemoveReportsForTeamFromList(string team, List<ReportEntry> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i].Team == team)
            {
                if (list[i].DisplayObject != null)
                    Destroy(list[i].DisplayObject);

                list[i].OnDestroyed -= HandleReportDestroyed;
                list.RemoveAt(i);
            }
        }
    }

    public void UpdateReportName(ReportEntry report, string newName)
    {
        UpdateReportNameInList(report, newName, reportEntriesUnseen);
        UpdateReportNameInList(report, newName, reportEntriesSeen);
    }

    private void UpdateReportNameInList(ReportEntry report, string newName, List<ReportEntry> list)
    {
        foreach (var entry in list)
        {
            if (entry == report && entry.DisplayObject != null)
            {
                TMP_Text text = entry.DisplayObject.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    entry.ReportName = newName;
                    text.text = $"{newName} - {entry.ActionType}";
                    text.color = GetPlayerColor(entry.Team);
                }
            }
        }
    }

    private void HandleReportDestroyed()
    {
        // currentlyOpenEntry should be the report that triggered this event
        if (currentlyOpenEntry != null)
        {
            RemoveReport(currentlyOpenEntry);
        }
    }

    public void ClearAllEntries()
    {
        foreach (var report in reportEntriesUnseen)
        {
            if (report.DisplayObject != null)
                Destroy(report.DisplayObject);
            report.OnDestroyed -= HandleReportDestroyed;
        }
        reportEntriesUnseen.Clear();

        foreach (var report in reportEntriesSeen)
        {
            if (report.DisplayObject != null)
                Destroy(report.DisplayObject);
            report.OnDestroyed -= HandleReportDestroyed;
        }
        reportEntriesSeen.Clear();

        CloseCurrentPopup();
    }

    public bool IsPointInScrollViewA(Vector2 screenPoint)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(viewportUnseen, screenPoint);
    }

    public bool IsPointInScrollViewB(Vector2 screenPoint)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(viewportSeen, screenPoint);
    }

    public void AddExistingEntry(ReportEntry report, bool toUnseen)
    {
        // Remove from both lists first to avoid duplicates
        reportEntriesUnseen.Remove(report);
        reportEntriesSeen.Remove(report);

        Transform targetContent = toUnseen ? contentPanelUnseen : contentPanelSeen;
        var targetList = toUnseen ? reportEntriesUnseen : reportEntriesSeen;

        if (report.DisplayObject == null)
        {
            GameObject entryObj = Instantiate(reportButtonPrefab, targetContent);

            TMP_Text text = entryObj.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = $"{report.ReportName} - {report.ActionType}";
                text.color = GetPlayerColor(report.Team);
            }

            Button button = entryObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    if (currentlyOpenEntry == report)
                    {
                        CloseCurrentPopup();
                    }
                    else
                    {
                        TryOpenReportPopup(report);
                    }
                });
            }

            DraggableReport draggable = entryObj.GetComponent<DraggableReport>();
            if (draggable != null)
            {
                draggable.OriginManager = this;
                draggable.ReportData = report;
            }
            else
            {
                Debug.LogWarning("DraggableReport component missing on reportButtonPrefab!");
            }

            report.DisplayObject = entryObj;

            report.OnDestroyed -= HandleReportDestroyed;
            report.OnDestroyed += HandleReportDestroyed;
        }
        else
        {
            report.DisplayObject.transform.SetParent(targetContent, false);

            TMP_Text text = report.DisplayObject.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = $"{report.ReportName} - {report.ActionType}";
                text.color = GetPlayerColor(report.Team);
            }
        }

        targetList.Add(report);
    }

    private Color GetPlayerColor(string team)
    {
        return team switch
        {
            "Player 1" => Color.red,
            "Player 2" => Color.blue,
            "Player 3" => Color.green,
            "Player 4" => Color.yellow,
            "Neutral" => Color.gray,
            _ => Color.white,
        };
    }
}