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
        HideContextMenu();

        GameObject selectedPrefab = item.IsUnit() ? contextMenuPrefabUnit : contextMenuPrefabItem;

        if (selectedPrefab == null)
        {
            Debug.LogError("Selected context menu prefab is null!");
            return;
        }

        currentContextMenuUI = Instantiate(selectedPrefab);
        currentContextMenuScript = currentContextMenuUI.GetComponent<ContextMenu3D>();

        if (currentContextMenuScript == null)
        {
            Debug.LogError("ContextMenu3D script missing on context menu prefab!");
            Destroy(currentContextMenuUI);
            return;
        }

        currentContextMenuScript.Init(item);

        Vector3 offset = new Vector3(15f, 30f, 0f);
        currentContextMenuUI.transform.position = position + offset;
        currentContextMenuUI.SetActive(true);

        if (item.IsUnit())
        {
            var visualizer = item.GetComponent<ViewRangeVisualizer>();
            if (visualizer != null)
                visualizer.ShowRing();

            var pathVisualizer = item.GetComponent<UnitMovement>();
            if (pathVisualizer != null)
                pathVisualizer.DisplaySavedPath();
        }
    }

    public void HideContextMenu()
    {
        if (currentContextMenuScript != null && currentContextMenuScript.Item != null && currentContextMenuScript.Item.IsUnit())
        {
            var visualizer = currentContextMenuScript.Item.GetComponent<ViewRangeVisualizer>();
            if (visualizer != null)
                visualizer.ClearRing();

            var pathVisualizer = currentContextMenuScript.Item.GetComponent<UnitMovement>();
            if (pathVisualizer != null)
                pathVisualizer.HidePath();
        }

        if (currentContextMenuUI != null)
        {
            Destroy(currentContextMenuUI);
            currentContextMenuUI = null;
            currentContextMenuScript = null;
        }
    }
}