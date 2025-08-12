using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionScrollViewManager : MonoBehaviour
{
    public static List<ActionScrollViewManager> AllScrollViews = new();

    [Header("UI References")]
    [SerializeField] private RectTransform contentPanelA;
    [SerializeField] private RectTransform viewportA;
    [SerializeField] private RectTransform contentPanelB;
    [SerializeField] private RectTransform viewportB;

    public Transform ContentPanelA => contentPanelA;
    public Transform ContentPanelB => contentPanelB;

    [SerializeField] private GameObject actionButtonPrefab;
    [SerializeField] private GameObject actionPopupPrefab;
    [SerializeField] private Transform popupParent;

    private List<ActionEntry> actionEntriesA = new();
    private List<ActionEntry> actionEntriesB = new();

    private static GameObject currentlyOpenPopup;
    private static ActionEntry currentlyOpenEntry;

    private void Awake()
    {
        AllScrollViews.Add(this);
    }

    private void OnDestroy()
    {
        AllScrollViews.Remove(this);
    }

    public void LogAction(PlaceableItemInstance unit, string actionDescription, string rollOutcome = "", bool toScrollViewA = true)
    {
        ActionEntry entry = new ActionEntry
        {
            Unit = unit,
            Team = unit.getTeam(),
            ActionDescription = actionDescription,
            Bias = ActionResolutionPopup.BiasType.Neutral,  // default bias
            RollOutcome = rollOutcome
        };

        Transform targetContent = toScrollViewA ? contentPanelA : contentPanelB;
        var targetList = toScrollViewA ? actionEntriesA : actionEntriesB;

        GameObject entryObj = Instantiate(actionButtonPrefab, targetContent);

        TMP_Text text = entryObj.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = $"{unit.getName()}: {entry.ActionDescription}";
            text.color = GetTeamColor(entry.Team);
        }

        DraggableAction draggable = entryObj.GetComponent<DraggableAction>();
        draggable.OriginManager = this;
        draggable.Unit = unit;
        draggable.ActionDescription = actionDescription;

        Button button = entryObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                // Toggle popup: close if already open for this entry, else open
                if (currentlyOpenEntry == entry)
                {
                    CloseCurrentPopup();
                }
                else
                {
                    TryOpenActionPopup(entry);
                }
            });
        }

        entry.DisplayObject = entryObj;
        targetList.Add(entry);

        unit.OnDestroyed += HandleUnitDestroyed;
    }

    private void TryOpenActionPopup(ActionEntry entry)
    {
        CloseCurrentPopup();

        GameObject popupInstance = Instantiate(actionPopupPrefab, popupParent);
        currentlyOpenPopup = popupInstance;
        currentlyOpenEntry = entry;

        RectTransform rectTransform = popupInstance.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
        }

        var popupScript = popupInstance.GetComponent<ActionResolutionPopup>();
        if (popupScript != null)
        {
            popupScript.Initialize(
                entry.Unit.getName(),
                entry.Team,
                "Action",
                entry.ActionDescription,
                entry.Bias,
                entry.RollOutcome
            );

            popupScript.OnDataChanged += (desc, bias, outcome) =>
            {
                entry.ActionDescription = desc;
                entry.Bias = bias;
                entry.RollOutcome = outcome;

                if (entry.DisplayObject != null)
                {
                    TMP_Text text = entry.DisplayObject.GetComponentInChildren<TMP_Text>();
                    if (text != null)
                    {
                        text.text = $"{entry.Unit.getName()}: {entry.ActionDescription}";
                        text.color = GetTeamColor(entry.Team);
                    }
                }
            };

            popupScript.OnCloseRequested += CloseCurrentPopup;
            popupScript.OnDeleteRequested += () =>
            {
                RemoveAction(entry.Unit, entry.ActionDescription);
                CloseCurrentPopup();
            };
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

    public void RemoveActionFromInternalLists(PlaceableItemInstance unit, string actionDescription)
    {
        RemoveActionFromListInternal(unit, actionDescription, actionEntriesA);
        RemoveActionFromListInternal(unit, actionDescription, actionEntriesB);
    }

    private void RemoveActionFromListInternal(PlaceableItemInstance unit, string actionDescription, List<ActionEntry> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var entry = list[i];
            if (entry.Unit == unit && entry.ActionDescription == actionDescription)
            {
                list.RemoveAt(i);
                return;
            }
        }
    }

    private bool RemoveActionFromList(PlaceableItemInstance unit, string actionDescription, List<ActionEntry> list, Transform parent)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var entry = list[i];
            if (entry.Unit == unit && entry.ActionDescription == actionDescription)
            {
                if (entry.DisplayObject != null)
                    Destroy(entry.DisplayObject);
                list.RemoveAt(i);

                unit.OnDestroyed -= HandleUnitDestroyed;
                return true;
            }
        }
        return false;
    }

    public void RemoveAction(PlaceableItemInstance unit, string actionDescription)
    {
        bool removed = RemoveActionFromList(unit, actionDescription, actionEntriesA, contentPanelA)
                    || RemoveActionFromList(unit, actionDescription, actionEntriesB, contentPanelB);
        if (removed)
        {
            unit.OnDestroyed -= HandleUnitDestroyed;
        }
    }

    public void RemoveActionsForUnit(PlaceableItemInstance unit)
    {
        RemoveActionsForUnitFromList(unit, actionEntriesA);
        RemoveActionsForUnitFromList(unit, actionEntriesB);
    }

    private void RemoveActionsForUnitFromList(PlaceableItemInstance unit, List<ActionEntry> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i].Unit == unit)
            {
                if (list[i].DisplayObject != null)
                    Destroy(list[i].DisplayObject);
                list.RemoveAt(i);
            }
        }
    }

    public void UpdateUnitName(PlaceableItemInstance unit, string newName)
    {
        UpdateUnitNameInList(unit, newName, actionEntriesA);
        UpdateUnitNameInList(unit, newName, actionEntriesB);
    }

    private void UpdateUnitNameInList(PlaceableItemInstance unit, string newName, List<ActionEntry> list)
    {
        foreach (var entry in list)
        {
            if (entry.Unit == unit && entry.DisplayObject != null)
            {
                TMP_Text text = entry.DisplayObject.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    string currentTeam = unit.getTeam();
                    entry.Team = currentTeam;
                    text.text = $"{newName}: {entry.ActionDescription}";
                    text.color = GetTeamColor(currentTeam);
                }
            }
        }
    }

    private void HandleUnitDestroyed(PlaceableItemInstance destroyedUnit)
    {
        RemoveActionsForUnit(destroyedUnit);
    }

    private Color GetTeamColor(string team)
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

    public void RemoveEntryDirectly(ActionEntry entry)
    {
        actionEntriesA.Remove(entry);
        actionEntriesB.Remove(entry);
    }

    public void AddExistingEntry(ActionEntry entry)
    {
        Transform parent = entry.DisplayObject.transform.parent;

        if (parent == ContentPanelA)
            actionEntriesA.Add(entry);
        else if (parent == ContentPanelB)
            actionEntriesB.Add(entry);
    }

    [System.Serializable]
    public class ActionEntry
    {
        public PlaceableItemInstance Unit;
        public string Team;
        public string ActionDescription; // was ActionText
        public ActionResolutionPopup.BiasType Bias; // store bias type
        public string RollOutcome; // current outcome
        public GameObject DisplayObject;
    }

    public bool IsPointInScrollViewA(Vector2 screenPoint)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(viewportA, screenPoint);
    }

    public bool IsPointInScrollViewB(Vector2 screenPoint)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(viewportB, screenPoint);
    }

    public void RemoveAllActions()
{
    // Close any open popup first
    CloseCurrentPopup();

    // Helper to clear a list and its objects
    void ClearList(List<ActionEntry> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var entry = list[i];
            if (entry.DisplayObject != null)
                Destroy(entry.DisplayObject);

            if (entry.Unit != null)
                entry.Unit.OnDestroyed -= HandleUnitDestroyed;

            list.RemoveAt(i);
        }
    }

    ClearList(actionEntriesA);
    ClearList(actionEntriesB);
}
}