using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttributeList : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform contentPanel;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] public RadioButtonUI attributeFilterRadio; // expects options: All / Proficiency / Fatigue / CommsClarity / Equipment

    private readonly List<string> attributeOrder = new List<string>
    {
        "Proficiency",
        "Fatigue",
        "CommsClarity",
        "Equipment"
    };

    private readonly List<GameObject> unitButtons = new List<GameObject>();
    private string currentSelectedAttribute = "All";

    private void Start()
    {
        if (attributeFilterRadio != null)
        {
            attributeFilterRadio.OnOptionSelected += OnAttributeFilterSelected;
        }
        else
        {
            Debug.LogWarning("AttributeList: attributeFilterRadio not assigned.");
        }

        Refresh();
    }

    public void Refresh()
    {
        var allUnits = ObjectPlacer.Instance?.GetAllUnits();
        if (allUnits == null) return;

        // Re-run the same logic as if the radio selected the current option
        ApplyFilterAndPopulate(currentSelectedAttribute, allUnits);
    }

    private void OnAttributeFilterSelected(string selectedAttribute)
    {
        currentSelectedAttribute = selectedAttribute;

        var allUnits = ObjectPlacer.Instance?.GetAllUnits();
        if (allUnits == null) return;

        ApplyFilterAndPopulate(selectedAttribute, allUnits);
    }

    private void ApplyFilterAndPopulate(string selectedAttribute, List<PlaceableItemInstance> units)
    {
        ClearList();

        IEnumerable<PlaceableItemInstance> ordered;

        if (selectedAttribute == "All")
        {
            // No specific attribute: show alphabetically
            ordered = units.OrderBy(u => u.getName());
        }
        else
        {
            // Sort by the selected attribute (desc), then by name
            ordered = units
                .OrderByDescending(u => GetAttrValue(u, selectedAttribute))
                .ThenBy(u => u.getName());
        }

        foreach (var u in ordered)
        {
            int value = selectedAttribute == "All" ? 0 : GetAttrValue(u, selectedAttribute);
            CreateAttributeRow(u.getName(), selectedAttribute, value);
        }
    }

    private int GetAttrValue(PlaceableItemInstance unit, string attribute)
    {
        switch (attribute)
        {
            case "Proficiency":   return Mathf.Clamp(unit.GetProficiency(), 1, 5);
            case "Fatigue":       return Mathf.Clamp(unit.GetFatigue(), 1, 5);
            case "CommsClarity":  return Mathf.Clamp(unit.GetCommsClarity(), 1, 5);
            case "Equipment":     return Mathf.Clamp(unit.GetEquipment(), 1, 5);
            default:              return 0;
        }
    }

    private void CreateAttributeRow(string unitName, string attribute, int value)
    {
        var go = Instantiate(buttonPrefab, contentPanel);

        // Text: show the value only when a specific attribute is selected
        var label = go.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = (attribute == "All")
                ? unitName
                : $"{unitName} — {attribute}: {value}/5";
        }

        // Color by attribute type (neutral when "All")
        var button = go.GetComponent<Button>();
        if (button != null)
        {
            Color c = GetAttributeColor(attribute);
            var cb = button.colors;
            cb.normalColor = c;
            cb.highlightedColor = c * 1.2f;
            cb.pressedColor = c * 0.8f;
            button.colors = cb;
        }

        unitButtons.Add(go);
    }

    private void ClearList()
    {
        foreach (var b in unitButtons) Destroy(b);
        unitButtons.Clear();
    }

    private Color GetAttributeColor(string attribute)
    {
        switch (attribute)
        {
            case "Proficiency":  return Color.cyan;
            case "Fatigue":      return Color.gray;
            case "CommsClarity": return Color.green;
            case "Equipment":    return Color.yellow;
            case "All":          return Color.white;
            default:             return Color.white;
        }
    }
}
