using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class ObjectPlacer : MonoBehaviour
{
    private GameObject selectedPrefab;
    private GameObject previewObject;
    private bool isPlacing = false;
    public LayerMask groundLayer;
    public static ObjectPlacer Instance { get; private set; }

    private PlaceableItem.ItemType selectedItemType;
    public TeamList teamList;
    private bool isRelocating = false;

    public List<PlaceableItemInstance> placedUnits = new List<PlaceableItemInstance>();
    private string currentFilterTeam = "All";

    public System.Action<PlaceableItemInstance> OnUnitPlaced;
    public System.Action<PlaceableItemInstance> OnUnitRemoved;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (teamList != null && teamList.teamFilterRadio != null)
        {
            teamList.teamFilterRadio.OnOptionSelected += UpdateTeamList;
            // Debug.Log("ObjectPlacer: Subscribed to teamFilterRadio.OnOptionSelected.");
        }
        else
        {
            Debug.LogWarning("ObjectPlacer: teamList or teamFilterRadio is not assigned.");
        }

        UpdateTeamList("All");
    }

    void Update()
    {
        if (isPlacing)
        {
            MovePreviewToMousePosition();

            if (Input.GetMouseButtonDown(0)) PlaceObject();
            if (Input.GetMouseButtonDown(1)) EndPlacement();
        }
    }

    public void SetSelectedPrefab(GameObject prefab, PlaceableItem.ItemType itemType, string existingName = null, string existingTeam = "Neutral")
    {

        selectedPrefab = prefab;
        selectedItemType = itemType;

        if (previewObject != null)
        {
            // Debug.Log("[ObjectPlacer] Destroying previous preview object.");
            Destroy(previewObject);
        }

        previewObject = Instantiate(selectedPrefab);
        // Debug.Log($"[ObjectPlacer] Preview object instantiated: {previewObject.name}");

        Collider col = previewObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            // Debug.Log("[ObjectPlacer] Disabled collider on preview object.");
        }

        SetMaterialTransparent(previewObject);
        // Debug.Log("[ObjectPlacer] Applied transparent material to preview object.");

        isPlacing = true;
        // Debug.Log("[ObjectPlacer] isPlacing set to TRUE.");

        var instance = previewObject.GetComponent<PlaceableItemInstance>() ?? previewObject.AddComponent<PlaceableItemInstance>();
        instance.Init(prefab, itemType, existingName ?? "NewObject");
        instance.setTeam(existingTeam);

        // Show ring for units during preview
        if (itemType == PlaceableItem.ItemType.Unit)
        {
            var visualizer = previewObject.GetComponent<ViewRangeVisualizer>();
            if (visualizer != null)
                visualizer.ShowRing();
        }

        // Debug.Log($"[ObjectPlacer] Preview object initialized with Name='{instance.getName()}', Team='{instance.getTeam()}', ItemType={instance.ItemType}");
    }

    public void SetRelocating(bool relocating)
    {
        isRelocating = relocating;
    }

    private void PlaceObject()
    {
        if (previewObject == null || selectedPrefab == null) return;

        GameObject placedObject = Instantiate(selectedPrefab, previewObject.transform.position, previewObject.transform.rotation);
        var previewInstance = previewObject.GetComponent<PlaceableItemInstance>();
        var instance = placedObject.GetComponent<PlaceableItemInstance>() ?? placedObject.AddComponent<PlaceableItemInstance>();

        string objectName = previewInstance != null ? previewInstance.getName() : "NewObject";
        string team = previewInstance != null ? previewInstance.getTeam() : "Neutral";

        instance.Init(selectedPrefab, selectedItemType, objectName);
        instance.setTeam(team);

        // Copy ViewRangeVisualizer settings from preview to placed object
        var previewVisualizer = previewObject.GetComponent<ViewRangeVisualizer>();
        var placedVisualizer = placedObject.GetComponent<ViewRangeVisualizer>();
        
        Debug.Log($"Preview visualizer: {(previewVisualizer != null ? "Found" : "NULL")}");
        Debug.Log($"Placed visualizer: {(placedVisualizer != null ? "Found" : "NULL")}");
        
        if (placedVisualizer != null)
        {
            if (previewVisualizer != null)
            {
                placedVisualizer.edgeSegmentPrefab = previewVisualizer.edgeSegmentPrefab;
                placedVisualizer.radius = previewVisualizer.radius;
                placedVisualizer.tileSize = previewVisualizer.tileSize;
            }
            placedVisualizer.ShowRing();
        }

        // This block ensures the instance is always tracked
        if (selectedItemType == PlaceableItem.ItemType.Unit)
        {
            placedUnits.Add(instance);
            OnUnitPlaced?.Invoke(instance);

            if (teamList != null)
            {
                teamList.AddUnit(objectName, team);
            }
        }

        isRelocating = false;
        EndPlacement();
    }

    public void RelocateUnit(PlaceableItemInstance unit)
    {
        RemoveUnit(unit); // clean from placedUnits and update TeamList
        unit.Delete();    // destroys the GameObject
        SetRelocating(true);
        SetSelectedPrefab(unit.getOriginalPrefab(), unit.ItemType, unit.getName(), unit.getTeam());
    }

    public void RemoveUnit(PlaceableItemInstance unit)
    {
        if (placedUnits.Contains(unit))
        {
            placedUnits.Remove(unit);
            OnUnitRemoved?.Invoke(unit); // Notify listeners
        }

        UpdateTeamList(currentFilterTeam); // Refresh list based on active filter
    }

    public List<PlaceableItemInstance> GetAllUnits()
    {
        return placedUnits;
    }

    private void MovePreviewToMousePosition()
    {
        if (previewObject == null) return;
        if (Camera.main == null) return;

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                Vector3 position = hit.point;
                Renderer rend = previewObject.GetComponentInChildren<Renderer>(true);
                if (rend != null) position.y += rend.bounds.extents.y;
                previewObject.transform.position = position;
            }
        }
    }

    private void EndPlacement()
    {
        isPlacing = false;

        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        previewObject = null;
        selectedPrefab = null;
    }

    private void SetMaterialTransparent(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            Material material = renderer.material;
            material.SetFloat("_Mode", 3);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }
    }

    public void AddUnit(PlaceableItemInstance unit)
    {
        if (!placedUnits.Contains(unit))
        {
            placedUnits.Add(unit);
            if (teamList != null)
            {
                teamList.AddUnit(unit.getName(), unit.getTeam());
            }
        }
    }

    public void UpdateTeamList(string selectedTeam)
    {
        currentFilterTeam = selectedTeam;
        if (teamList == null) return;

        var filtered = selectedTeam == "All"
            ? placedUnits
            : placedUnits.Where(u => u.getTeam() == selectedTeam).ToList();

        teamList.PopulateList(filtered);
    }

    public void UpdateTeamList()
    {
        if (teamList != null)
        {
            teamList.PopulateList(placedUnits);
        }
    }

    public void setGroundLayerForPlacement(LayerMask newGroundLayer)
    {
        groundLayer = newGroundLayer;
    }

    public void LoadUnits(List<PlaceableItemInstance> units)
    {
        placedUnits = new List<PlaceableItemInstance>(units);

        if (teamList != null)
        {
            // Instead of just populating list with all units, filter by currentFilterTeam
            UpdateTeamList(string.IsNullOrEmpty(currentFilterTeam) ? "All" : currentFilterTeam);
        }
    }
}