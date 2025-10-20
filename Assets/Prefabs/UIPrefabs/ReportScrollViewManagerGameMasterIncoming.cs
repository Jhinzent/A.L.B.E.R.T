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

    [SerializeField] private RectTransform contentPanelArchived; // C panel
    [SerializeField] private RectTransform viewportArchived;

    [SerializeField] private PlayerReportOutputScrollViewManager playerReportOutputScrollViewManager;

    public Transform ContentPanelA => contentPanelUnseen;
    public Transform ContentPanelB => contentPanelSeen;
    public Transform ContentPanelC => contentPanelArchived;

    public RectTransform ViewportA => viewportUnseen;
    public RectTransform ViewportB => viewportSeen;
    public RectTransform ViewportC => viewportArchived;

    [SerializeField] private GameObject reportButtonPrefab;
    [SerializeField] private GameObject reportPopupPrefab;
    [SerializeField] private Transform popupParent;

    private List<ReportEntry> reportEntriesUnseen = new();
    private List<ReportEntry> reportEntriesSeen = new();
    private List<ReportEntry> reportEntriesArchived = new();

    // NEW: keep manager-owned display objects per report, so multiple managers can have their own UI
    private readonly Dictionary<ReportEntry, List<GameObject>> reportDisplayObjects = new();

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

        // Debug.Log($"[LoadAllReports] Got {allReports?.Count ?? 0} reports from PlayerReportOutputScrollViewManager.");
        foreach (var report in allReports)
        {
            AddReport(report, 0);
        }
    }

    public void AddReport(ReportEntry report, int targetPanel = 0)
    {
        if (report == null)
        {
            Debug.LogError("[AddReport] Report is NULL! Aborting.");
            return;
        }

        Transform targetContent = targetPanel switch
        {
            0 => contentPanelUnseen,
            1 => contentPanelSeen,
            2 => contentPanelArchived,
            _ => contentPanelUnseen
        };
        
        var targetList = targetPanel switch
        {
            0 => reportEntriesUnseen,
            1 => reportEntriesSeen,
            2 => reportEntriesArchived,
            _ => reportEntriesUnseen
        };

        // Debug.Log($"[AddReport] Target content panel = '{targetContent?.name}', Target list size before add = {targetList.Count}");

        // Prevent duplicates in this manager's list
        if (targetList.Contains(report))
        {
            Debug.LogWarning($"[AddReport] Duplicate detected in THIS list for '{report.ReportName}'. Skipping.");
            return;
        }

        if (reportButtonPrefab == null)
        {
            Debug.LogError("[AddReport] reportButtonPrefab is NULL! Cannot create UI element.");
            return;
        }

        // Instantiate a new UI object for this manager (do NOT touch report.DisplayObject)
        GameObject entryObj = Instantiate(reportButtonPrefab, targetContent);
        // bug.Log($"[AddReport] Instantiated new report button for '{report.ReportName}' under '{targetContent?.name}'.");

        TMP_Text text = entryObj.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = $"{report.ReportName}";
            text.color = GetPlayerColor(report.Team);
            // Debug.Log($"[AddReport] Set text='{report.ReportName}', color='{text.color}'.");
        }
        else
        {
            Debug.LogWarning("[AddReport] No TMP_Text found in prefab!");
        }

        Button button = entryObj.GetComponent<Button>();
        if (button != null)
        {
            // Ensure we don't stack listeners if somehow reusing UI (we're creating new ones though)
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                // Debug.Log($"[AddReport-ButtonClick] '{report.ReportName}' clicked.");
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
        else
        {
            Debug.LogWarning("[AddReport] No Button component found!");
        }

        // Add DraggableReport component if missing, then assign its data
        DraggableReport draggable = entryObj.GetComponent<DraggableReport>();
        if (draggable == null)
        {
            draggable = entryObj.AddComponent<DraggableReport>();
            // Debug.Log("[AddReport] DraggableReport component added.");
        }
        draggable.OriginManager = this;
        draggable.ReportData = report;
        // Debug.Log("[AddReport] DraggableReport assigned.");

        // Register this manager's UI instance for the report
        if (!reportDisplayObjects.TryGetValue(report, out var list))
        {
            list = new List<GameObject>();
            reportDisplayObjects[report] = list;
        }
        list.Add(entryObj);
        // Debug.Log($"[AddReport] Registered display object for '{report.ReportName}' (manager-owned instances = {list.Count}).");

        // Add to this manager's logical list
        targetList.Add(report);
        // Debug.Log($"[AddReport] Added '{report.ReportName}' to list. New size = {targetList.Count}");

        // Subscribe safely to avoid multiple subscriptions
        report.OnDestroyed -= HandleReportDestroyed;
        report.OnDestroyed += HandleReportDestroyed;
        // Debug.Log($"[AddReport] Subscribed to OnDestroyed for '{report.ReportName}'.");
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
            // Initialize with report entry
            popupScript.Initialize(report);

            // Subscribe to events
            popupScript.OnCloseRequested += CloseCurrentPopup;
            popupScript.OnTitleChanged += (newTitle) => UpdateReportName(report, newTitle);
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
        // Debug.Log($"[RemoveReport] Attempting to remove report '{report?.ReportName ?? "NULL"}' from this manager.");

        bool removed = RemoveReportFromList(report, reportEntriesUnseen, contentPanelUnseen)
                    || RemoveReportFromList(report, reportEntriesSeen, contentPanelSeen)
                    || RemoveReportFromList(report, reportEntriesArchived, contentPanelArchived);

        if (removed)
        {
            //  Debug.Log($"[RemoveReport] '{report.ReportName}' removed from at least one list in this manager.");

            // Unsubscribe event (manager no longer cares)
            report.OnDestroyed -= HandleReportDestroyed;

            // We do NOT touch report.DisplayObject here (other managers might use it).
            // We cleaned up manager-owned UI in RemoveReportFromList already.

            // If the removed report was the currently open one, close popup
            if (currentlyOpenEntry == report)
            {
                CloseCurrentPopup();
            }
        }
        else
        {
            Debug.LogWarning($"[RemoveReport] Report '{report?.ReportName ?? "NULL"}' was not found in this manager's lists.");
        }
    }

    private bool RemoveReportFromList(ReportEntry report, List<ReportEntry> list, Transform parent)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] == report)
            {
                // Destroy only this manager's UI instances for this report
                DestroyManagerOwnedDisplayObjects(report);

                list.RemoveAt(i);

                // Debug.Log($"[RemoveReportFromList] Removed report '{report.ReportName}' from list. Remaining in list = {list.Count}");
                return true;
            }
        }
        return false;
    }

    public void RemoveReportsForTeam(string team)
    {
        RemoveReportsForTeamFromList(team, reportEntriesUnseen);
        RemoveReportsForTeamFromList(team, reportEntriesSeen);
        RemoveReportsForTeamFromList(team, reportEntriesArchived);
    }

    private void RemoveReportsForTeamFromList(string team, List<ReportEntry> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i].Team == team)
            {
                var report = list[i];
                // Debug.Log($"[RemoveReportsForTeamFromList] Removing '{report.ReportName}' for team '{team}' from this manager.");

                // Destroy manager-owned UI
                DestroyManagerOwnedDisplayObjects(report);

                report.OnDestroyed -= HandleReportDestroyed;
                list.RemoveAt(i);
            }
        }
    }

    public void UpdateReportName(ReportEntry report, string newName)
    {
        UpdateReportNameInList(report, newName, reportEntriesUnseen);
        UpdateReportNameInList(report, newName, reportEntriesSeen);
        UpdateReportNameInList(report, newName, reportEntriesArchived);
    }

    private void UpdateReportNameInList(ReportEntry report, string newName, List<ReportEntry> list)
    {
        foreach (var entry in list)
        {
            if (entry == report)
            {
                // Update model
                entry.ReportName = newName;

                // Update all manager-owned UI instances for this report
                if (reportDisplayObjects.TryGetValue(report, out var objs))
                {
                    foreach (var obj in objs)
                    {
                        if (obj == null) continue;
                        TMP_Text text = obj.GetComponentInChildren<TMP_Text>();
                        if (text != null)
                        {
                            text.text = $"{newName}";
                            text.color = GetPlayerColor(entry.Team);
                        }
                    }
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
        // Debug.Log("[ClearAllEntries] Clearing unseen entries and manager-owned UI.");
        foreach (var report in reportEntriesUnseen)
        {
            // Destroy manager-owned UI instances
            DestroyManagerOwnedDisplayObjects(report);
            report.OnDestroyed -= HandleReportDestroyed;
        }
        reportEntriesUnseen.Clear();

        // Debug.Log("[ClearAllEntries] Clearing seen entries and manager-owned UI.");
        foreach (var report in reportEntriesSeen)
        {
            DestroyManagerOwnedDisplayObjects(report);
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

    public bool IsPointInScrollViewC(Vector2 screenPoint)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(viewportArchived, screenPoint);
    }

    public void AddExistingEntry(ReportEntry report, int targetPanel = 0)
    {
        // Remove from all lists first to avoid duplicates in this manager
        reportEntriesUnseen.Remove(report);
        reportEntriesSeen.Remove(report);
        reportEntriesArchived.Remove(report);

        Transform targetContent = targetPanel switch
        {
            0 => contentPanelUnseen,
            1 => contentPanelSeen,
            2 => contentPanelArchived,
            _ => contentPanelUnseen
        };
        
        var targetList = targetPanel switch
        {
            0 => reportEntriesUnseen,
            1 => reportEntriesSeen,
            2 => reportEntriesArchived,
            _ => reportEntriesUnseen
        };

        if (reportButtonPrefab == null)
        {
            Debug.LogError("[AddExistingEntry] reportButtonPrefab is NULL! Cannot create UI element.");
            return;
        }

        // Always create a manager-owned UI instance (don't rely on report.DisplayObject)
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
            button.onClick.RemoveAllListeners();
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
        if (draggable == null)
        {
            draggable = entryObj.AddComponent<DraggableReport>();
            // Debug.Log("[AddExistingEntry] DraggableReport component added.");
        }
        draggable.OriginManager = this;
        draggable.ReportData = report;

        // register manager-owned instance
        if (!reportDisplayObjects.TryGetValue(report, out var list))
        {
            list = new List<GameObject>();
            reportDisplayObjects[report] = list;
        }
        list.Add(entryObj);

        report.OnDestroyed -= HandleReportDestroyed;
        report.OnDestroyed += HandleReportDestroyed;

        targetList.Add(report);
        // Debug.Log($"[AddExistingEntry] Added existing report '{report.ReportName}' to this manager. Manager-owned instances = {list.Count}");
    }

    private void DestroyManagerOwnedDisplayObjects(ReportEntry report)
    {
        if (report == null) return;

        if (reportDisplayObjects.TryGetValue(report, out var objs))
        {
            // Debug.Log($"[DestroyManagerOwnedDisplayObjects] Destroying {objs.Count} manager-owned UI objects for '{report.ReportName}'.");
            foreach (var obj in objs)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            reportDisplayObjects.Remove(report);
        }
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