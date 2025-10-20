using UnityEngine;

public class ContextMenuManager : MonoBehaviour
{
    public static ContextMenuManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject contextMenuPrefabItem;
    public GameObject contextMenuPrefabUnit;
    public ActionScrollViewManager ActionScrollView;

    [Header("References")]
    [SerializeField] private TeamList teamList; // <-- Assign via Inspector

    public TeamList TeamList => teamList;

    private GameObject currentContextMenuUI;
    private ContextMenu3D currentContextMenuScript;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        var visualizer = GetComponent<ViewRangeVisualizer>();
        if (visualizer != null)
            visualizer.ShowRing();
    }

    public void ShowContextMenu(PlaceableItemInstance item, Vector3 position)
    {
        Debug.Log($"[ContextMenuManager] ShowContextMenu called for item: {item?.name} at position: {position}");
        Debug.Log($"[ContextMenuManager] Item IsUnit: {item?.IsUnit()}, ItemType: {item?.ItemType}");
        Debug.Log($"[ContextMenuManager] contextMenuPrefabUnit: {contextMenuPrefabUnit?.name}, contextMenuPrefabItem: {contextMenuPrefabItem?.name}");
        
        HideContextMenu();

        GameObject selectedPrefab = item.IsUnit() ? contextMenuPrefabUnit : contextMenuPrefabItem;
        Debug.Log($"[ContextMenuManager] Selected prefab: {selectedPrefab?.name}");

        if (selectedPrefab == null)
        {
            // Debug.LogError($"[ContextMenuManager] Selected context menu prefab is null! IsUnit: {item?.IsUnit()}");
            return;
        }

        currentContextMenuUI = Instantiate(selectedPrefab);
        // Debug.Log($"[ContextMenuManager] Context menu UI instantiated: {currentContextMenuUI?.name}");
        
        currentContextMenuScript = currentContextMenuUI.GetComponent<ContextMenu3D>();

        if (currentContextMenuScript == null)
        {
            // Debug.LogError($"[ContextMenuManager] ContextMenu3D script missing on context menu prefab: {selectedPrefab.name}!");
            Destroy(currentContextMenuUI);
            return;
        }

        // Debug.Log($"[ContextMenuManager] Initializing context menu with item: {item?.name}");
        currentContextMenuScript.Init(item);

        Vector3 offset = new Vector3(15f, 30f, 0f);
        currentContextMenuUI.transform.position = position + offset;
        currentContextMenuUI.SetActive(true);
        // Debug.Log($"[ContextMenuManager] Context menu activated at position: {currentContextMenuUI.transform.position}");

        if (item.IsUnit())
        {
            var visualizer = item.GetComponent<ViewRangeVisualizer>();
            if (visualizer != null)
            {
                // Debug.Log($"[ContextMenuManager] ShowContextMenu calling ShowRing on {item.name}");
                visualizer.ShowRing();
            }
            else
            {
                // Debug.Log($"[ContextMenuManager] No ViewRangeVisualizer found on unit {item.name}");
            }

            var pathVisualizer = item.GetComponent<UnitMovement>();
            if (pathVisualizer != null)
                pathVisualizer.DisplaySavedPath();
        }
    }

    public void HideContextMenu()
    {
        // Debug.Log($"[ContextMenuManager] HideContextMenu called. Current menu exists: {currentContextMenuUI != null}");
        
        if (currentContextMenuScript != null && currentContextMenuScript.Item != null && currentContextMenuScript.Item.IsUnit())
        {
            var visualizer = currentContextMenuScript.Item.GetComponent<ViewRangeVisualizer>();
            if (visualizer != null)
            {
                // Debug.Log($"[ContextMenuManager] HideContextMenu calling ClearRing on {currentContextMenuScript.Item.name}");
                visualizer.ClearRing();
            }

            var pathVisualizer = currentContextMenuScript.Item.GetComponent<UnitMovement>();
            if (pathVisualizer != null)
                pathVisualizer.HidePath();
        }

        if (currentContextMenuUI != null)
        {
            // Debug.Log($"[ContextMenuManager] Destroying context menu UI: {currentContextMenuUI.name}");
            Destroy(currentContextMenuUI);
            currentContextMenuUI = null;
            currentContextMenuScript = null;
        }
    }
}