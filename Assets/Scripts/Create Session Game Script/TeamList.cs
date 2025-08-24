using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamList : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform contentPanel;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] public RadioButtonUI teamFilterRadio;

    private List<GameObject> unitButtons = new List<GameObject>();

    private List<string> teamOrder = new List<string>
    {
        "Neutral",
        "Player 1",
        "Player 2",
        "Player 3",
        "Player 4"
    };

    private string currentSelectedTeam = "All";

    void Start()
    {
        if (teamFilterRadio != null)
        {
            // Debug.Log("TeamList: Subscribing to RadioButtonUI.OnOptionSelected");
            teamFilterRadio.OnOptionSelected += OnTeamFilterSelected;
        }
        else
        {
            Debug.LogWarning("TeamList: teamFilterRadio not assigned.");
        }

        OnTeamFilterSelected(currentSelectedTeam);
    }

    private void OnTeamFilterSelected(string selectedTeam)
    {
        //Debug.Log($"TeamList: RadioButton selected team = '{selectedTeam}'");
        currentSelectedTeam = selectedTeam;

        List<PlaceableItemInstance> allUnits = ObjectPlacer.Instance?.GetAllUnits();
        if (allUnits == null)
        {
            // Debug.LogWarning("TeamList: ObjectPlacer.GetAllUnits() returned null");
            return;
        }

        List<PlaceableItemInstance> filtered = selectedTeam == "All"
            ? new List<PlaceableItemInstance>(allUnits)
            : allUnits.Where(u => u.getTeam() == selectedTeam).ToList();

        PopulateList(filtered);
    }

    public void PopulateList(List<PlaceableItemInstance> units)
    {
        // Debug.Log($"TeamList: PopulateList called with {units.Count} units");

        ClearList();

        var sortedUnits = units.OrderBy(u => GetTeamOrderIndex(u.getTeam()))
                               .ThenBy(u => u.getName());

        foreach (var unit in sortedUnits)
        {
            // Debug.Log($"TeamList: Adding unit {unit.getName()} from team {unit.getTeam()}");
            CreateUnitButton(unit.getName(), unit.getTeam());
        }
    }

    public void AddUnit(string unitName, string team)
    {
        //Debug.Log($"TeamList: AddUnit called for {unitName} (team='{team}'), currentSelectedTeam='{currentSelectedTeam}'");
        if (currentSelectedTeam == "All" || currentSelectedTeam == team)
        {
            //Debug.Log($"TeamList: Refreshing list for {unitName}");
            OnTeamFilterSelected(currentSelectedTeam);
        }
        else
        {
            Debug.Log($"TeamList: Not refreshing - team '{team}' doesn't match filter '{currentSelectedTeam}'");
        }
    }

    public void RemoveUnit(string unitName)
    {
        // Debug.Log($"TeamList: RemoveUnit called for {unitName}");

        for (int i = 0; i < unitButtons.Count; i++)
        {
            var text = unitButtons[i].GetComponentInChildren<TMP_Text>();
            if (text != null && text.text == unitName)
            {
                Destroy(unitButtons[i]);
                unitButtons.RemoveAt(i);
                // Debug.Log($"TeamList: Removed unit button for {unitName}");
                break;
            }
        }
    }

    private void CreateUnitButton(string unitName, string team)
    {
        GameObject buttonObj = Instantiate(buttonPrefab, contentPanel);
        ConfigureButton(buttonObj, unitName, team);
        unitButtons.Add(buttonObj);
    }

    private void CreateUnitButtonSorted(string unitName, string team)
    {
        // Only show the unit if it matches the current filter
        if (currentSelectedTeam != "All" && currentSelectedTeam != team)
        {
            return; // Don't create button if it doesn't match current filter
        }

        GameObject buttonObj = Instantiate(buttonPrefab);
        ConfigureButton(buttonObj, unitName, team);

        int insertIndex = FindInsertIndex(team, unitName);

        buttonObj.transform.SetParent(contentPanel, false);
        buttonObj.transform.SetSiblingIndex(insertIndex);
        unitButtons.Insert(insertIndex, buttonObj);
    }

    private void ConfigureButton(GameObject buttonObj, string unitName, string team)
    {
        Button button = buttonObj.GetComponent<Button>();
        TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
            buttonText.text = unitName;

        Color teamColor = GetTeamColor(team);
        ColorBlock cb = button.colors;
        cb.normalColor = teamColor;
        cb.highlightedColor = teamColor * 1.2f;
        cb.pressedColor = teamColor * 0.8f;
        button.colors = cb;
    }

    private int FindInsertIndex(string team, string unitName)
    {
        int newTeamIndex = GetTeamOrderIndex(team);

        for (int i = 0; i < unitButtons.Count; i++)
        {
            var text = unitButtons[i].GetComponentInChildren<TMP_Text>();
            var existingTeamColor = unitButtons[i].GetComponent<Button>().colors.normalColor;
            int existingTeamIndex = GetTeamOrderIndex(GetTeamNameByColor(existingTeamColor));

            if (newTeamIndex < existingTeamIndex ||
               (newTeamIndex == existingTeamIndex && string.Compare(unitName, text.text) < 0))
            {
                return i;
            }
        }

        return unitButtons.Count;
    }

    private int GetTeamOrderIndex(string team)
    {
        int index = teamOrder.IndexOf(team);
        return index >= 0 ? index : teamOrder.Count;
    }

    private string GetTeamNameByColor(Color color)
    {
        if (color == Color.red) return "Player 1";
        if (color == Color.blue) return "Player 2";
        if (color == Color.green) return "Player 3";
        if (color == Color.yellow) return "Player 4";
        if (color == Color.gray) return "Neutral";
        return "Unknown";
    }

    private void ClearList()
    {
        foreach (var btn in unitButtons)
        {
            Destroy(btn);
        }
        unitButtons.Clear();
    }

    private Color GetTeamColor(string team)
    {
        switch (team)
        {
            case "Neutral": return Color.gray;
            case "Player 1": return Color.red;
            case "Player 2": return Color.blue;
            case "Player 3": return Color.green;
            case "Player 4": return Color.yellow;
            default: return Color.white;
        }
    }
}