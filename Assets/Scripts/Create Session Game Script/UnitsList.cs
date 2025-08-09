using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitList : MonoBehaviour
{
    public List<PlaceableItem> units = new List<PlaceableItem>();
    public GameObject scrollViewUnitPrefab; // Assign the UnitTemplate prefab
    public Transform contentParent; // Assign the Content object in the Scroll View
    public ObjectPlacer objectPlacer;

    void Start()
    {
        PopulateScrollView();
    }

    void PopulateScrollView()
    {
        if (units == null || units.Count == 0)
        {
            Debug.LogError("UnitList or units is null/empty!");
            return;
        }

        foreach (var unit in units)
        {
            GameObject instantiatedUnit = Instantiate(scrollViewUnitPrefab, contentParent);
            ObjectInScrollView newScrollViewItem = instantiatedUnit.GetComponent<ObjectInScrollView>();

            newScrollViewItem.setAssociatedObject(unit.itemPrefab);
            newScrollViewItem.setTextComponent(unit.itemName);
            newScrollViewItem.setSpriteComponent(unit.itemImage);

            // Capture variables locally to avoid closure issues
            GameObject prefab = unit.itemPrefab;
            PlaceableItem.ItemType itemType = unit.itemType;

            Button scrollViewItemButton = newScrollViewItem.getButtonComponent();
            scrollViewItemButton.onClick.AddListener(() => objectPlacer.SetSelectedPrefab(prefab, itemType));
        }
    }
}