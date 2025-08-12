using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerReportScrollViewManager : MonoBehaviour
{
    [Header("Player ScrollView UI References")]
    [Tooltip("Each element corresponds to a player's scrollview content panel.")]
    public List<RectTransform> playerScrollViewContents = new();

    [Tooltip("Each element corresponds to the popup parent for that player's scene.")]
    public List<Transform> popupParents = new();

    [Header("Prefabs")]
    [SerializeField] private GameObject playerReportItemPrefab; // Player version of the report item
    [SerializeField] private GameObject reportPopupPrefab;      // Player-specific popup

    private readonly List<ReportEntry> playerReportEntries = new();

    private static GameObject currentlyOpenPopup;
    private static ReportEntry currentlyOpenEntry;

    public void ReceiveReports(List<ReportEntry> reportsFromGameMaster)
    {
        // Debug.Log($"[PlayerManager] Receiving {reportsFromGameMaster.Count} reports from GameMaster.");

        ClearAll();

        foreach (var entry in reportsFromGameMaster)
        {
            int playerIndex = GetPlayerIndex(entry.Team);

            if (playerIndex >= 0 && playerIndex < playerScrollViewContents.Count)
            {
                // Debug.Log($"[PlayerManager] Creating entry for {entry.ReportName} in Player {playerIndex + 1}'s scrollview.");

                GameObject entryObj = Instantiate(playerReportItemPrefab, playerScrollViewContents[playerIndex]);

                TMP_Text text = entryObj.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.text = entry.ReportName;
                    text.color = GetTeamColor(entry.Team);
                }

                Button button = entryObj.GetComponent<Button>();
                if (button != null)
                {
                    // Debug.Log($"[PlayerManager] Adding click listener to {entry.ReportName}");
                    int capturedIndex = playerIndex;
                    button.onClick.AddListener(() => OnReportClicked(entry, capturedIndex));
                }
                else
                {
                    Debug.LogWarning($"[PlayerManager] No Button component found on {entry.ReportName}'s prefab!");
                }

                entry.DisplayObject = entryObj;
                playerReportEntries.Add(entry);
            }
            else
            {
                Debug.LogWarning($"[PlayerManager] Could not find scrollview for team {entry.Team}");
            }
        }
    }

    private void OnReportClicked(ReportEntry entry, int playerIndex)
    {
        // Debug.Log($"[PlayerManager] Clicked on {entry.ReportName} for Player {playerIndex + 1}");

        if (currentlyOpenEntry == entry)
        {
           //  Debug.Log("[PlayerManager] Closing currently open popup.");
            CloseCurrentPopup();
        }
        else
        {
            // Debug.Log("[PlayerManager] Opening popup for clicked entry.");
            OpenPopupForEntry(entry, playerIndex);
        }
    }

    private void OpenPopupForEntry(ReportEntry entry, int playerIndex)
    {
        CloseCurrentPopup();

        // Debug.Log($"[PlayerManager] Instantiating popup for {entry.ReportName} in Player {playerIndex + 1}'s scene.");

        Transform parent = popupParents.Count > playerIndex ? popupParents[playerIndex] : transform;
        if (parent == null)
        {
            Debug.LogWarning("[PlayerManager] Popup parent is null! Using this transform as fallback.");
            parent = transform;
        }

        GameObject popupInstance = Instantiate(reportPopupPrefab, parent);
        currentlyOpenPopup = popupInstance;
        currentlyOpenEntry = entry;

        var popupScript = popupInstance.GetComponent<ReportPopupPlayer>();
        if (popupScript != null)
        {
            // Debug.Log("[PlayerManager] Initializing ReportPopupPlayer...");
            popupScript.Initialize(entry.Description, entry.ActionType);

            // Subscribe to the close event
            popupScript.OnCloseRequested += CloseCurrentPopup;
        }
        else
        {
            Debug.LogWarning("[PlayerManager] ReportPopupPlayer component not found on popup prefab!");
        }
    }

    public void CloseCurrentPopup()
    {
        if (currentlyOpenPopup != null)
        {
            // Debug.Log("[PlayerManager] Destroying current popup.");
            Destroy(currentlyOpenPopup);
            currentlyOpenPopup = null;
            currentlyOpenEntry = null;
        }
    }

    private void ClearAll()
    {
        // Debug.Log("[PlayerManager] Clearing all current entries.");
        foreach (var entry in playerReportEntries)
        {
            if (entry.DisplayObject != null)
            {
                Destroy(entry.DisplayObject);
            }
        }
        playerReportEntries.Clear();
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

    private int GetPlayerIndex(string team)
    {
        return team switch
        {
            "Player 1" => 0,
            "Player 2" => 1,
            "Player 3" => 2,
            "Player 4" => 3,
            _ => -1
        };
    }
}