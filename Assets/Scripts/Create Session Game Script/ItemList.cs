using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemList : MonoBehaviour
{
    public List<PlaceableItem> items = new List<PlaceableItem>();
    public GameObject scrollViewItemPrefab; // Assign the ItemTemplate prefab
    public Transform contentParent; // Assign the Content object in the Scroll View
    public ObjectPlacer objectPlacer;

    void Start()
    {
        PopulateScrollView();
    }

    void PopulateScrollView()
    {
        if (items == null || items == null || items.Count == 0)
        {
            Debug.LogError("ItemList or Items is null/empty!");
            return;
        }

        foreach (var item in items)
        {
            GameObject instantiatedItem = Instantiate(scrollViewItemPrefab, contentParent);
            ObjectInScrollView newScrollViewItem = instantiatedItem.GetComponent<ObjectInScrollView>();

            newScrollViewItem.setAssociatedObject(item.itemPrefab);
            newScrollViewItem.setTextComponent(item.itemName);
            newScrollViewItem.setSpriteComponent(item.itemImage);

            // Correctly capture the item in a local variable for use inside the lambda
            PlaceableItem.ItemType itemType = item.itemType;

            Button scrollViewItemButton = newScrollViewItem.getButtonComponent();
            scrollViewItemButton.onClick.AddListener(() =>
                objectPlacer.SetSelectedPrefab(newScrollViewItem.getAssociatedObject(), itemType));
        }
    }
}