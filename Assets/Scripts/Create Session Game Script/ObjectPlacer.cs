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
    private int[] relocatingAttributes; // [proficiency, fatigue, commsClarity, equipment]

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

            if (Input.GetMouseButtonDown(0))
            {
                // Only place if not over UI and raycast hits ground
                bool isOverUI = EventSystem.current.IsPointerOverGameObject();
                if (!isOverUI && CanPlaceAtCurrentPosition())
                {
                    PlaceObject();
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                EndPlacement();
            }
        }
    }

    private bool CanPlaceAtCurrentPosition()
    {
        if (Camera.main == null) return false;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer);
    }

    public void SetSelectedPrefab(GameObject prefab, PlaceableItem.ItemType itemType, string existingName = null, string existingTeam = "Neutral")
    {
        // Check if prefab is null or destroyed
        if (prefab == null)
        {
            Debug.LogError($"[ObjectPlacer] SetSelectedPrefab called with null prefab!");
            return;
        }

        selectedPrefab = prefab;
        selectedItemType = itemType;

        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        previewObject = Instantiate(selectedPrefab);
        Vector3 originalScale = previewObject.transform.localScale;

        Collider col = previewObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        SetMaterialTransparent(previewObject);
        previewObject.transform.localScale = originalScale; // Preserve original scale

        isPlacing = true;
        // Debug.Log("[ObjectPlacer] isPlacing set to TRUE.");

        // Get or add PlaceableItemInstance and ensure it's properly initialized
        var instance = previewObject.GetComponent<PlaceableItemInstance>();
        GameObject originalPrefabBackup = null;
        
        if (instance != null)
        {
            // Preserve inspector-assigned OriginalPrefab if it exists
            originalPrefabBackup = instance.OriginalPrefab;
            Debug.Log($"[ObjectPlacer] Existing instance found with OriginalPrefab: {originalPrefabBackup?.name}");
        }
        else
        {
            instance = previewObject.AddComponent<PlaceableItemInstance>();
            Debug.Log($"[ObjectPlacer] Created new PlaceableItemInstance component");
        }
        
        // Always use the parameter (which should be the actual prefab asset)
        // Don't use originalPrefabBackup as it might be a clone
        instance.Init(prefab, itemType, existingName ?? "NewObject");
        instance.setTeam(existingTeam);
        
        Debug.Log($"[ObjectPlacer] Preview initialized - OriginalPrefab: {instance.getOriginalPrefab()?.name}, ItemType: {instance.ItemType}, selectedPrefab: {selectedPrefab?.name}");

        // Show ring for units during preview
        if (itemType == PlaceableItem.ItemType.Unit)
        {
            var visualizer = previewObject.GetComponent<ViewRangeVisualizer>();
            if (visualizer != null)
            {
                //Debug.Log($"[ObjectPlacer] Starting coroutine for preview ring on {previewObject.name}");
                // Wait for position to be set before showing ring
                StartCoroutine(ShowPreviewRingAfterPosition(visualizer));
            }
            else
            {
                //Debug.Log($"[ObjectPlacer] No ViewRangeVisualizer found on preview object {previewObject.name}");
            }
        }

        // Debug.Log($"[ObjectPlacer] Preview object initialized with Name='{instance.getName()}', Team='{instance.getTeam()}', ItemType={instance.ItemType}");
    }

    public void SetRelocating(bool relocating)
    {
        isRelocating = relocating;
    }

    private void PlaceObject()
    {
        if (previewObject == null)
        {
            Debug.LogWarning($"[ObjectPlacer] PlaceObject failed - previewObject is null");
            return;
        }
        
        // Store values before potential destruction
        var previewComp = previewObject.GetComponent<PlaceableItemInstance>();
        GameObject prefabToPlace = previewComp?.getOriginalPrefab() ?? selectedPrefab;
        Vector3 previewPosition = previewObject.transform.position;
        Quaternion previewRotation = previewObject.transform.rotation;
        ViewRangeVisualizer previewVisualizer = previewObject.GetComponent<ViewRangeVisualizer>();
        
        if (prefabToPlace == null)
        {
            Debug.LogWarning($"[ObjectPlacer] PlaceObject failed - no prefab available (preview: {previewComp?.getOriginalPrefab()?.name}, selected: {selectedPrefab?.name})");
            return;
        }
        
        GameObject placedObject = Instantiate(prefabToPlace, previewPosition, previewRotation);
        
        // Ensure proper scale (in case preview was affected)
        placedObject.transform.localScale = prefabToPlace.transform.localScale;
        
        // Temporarily disable collider to prevent premature clicks
        Collider col = placedObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        var instance = placedObject.GetComponent<PlaceableItemInstance>() ?? placedObject.AddComponent<PlaceableItemInstance>();

        string objectName = previewComp != null ? previewComp.getName() : "NewObject";
        string team = previewComp != null ? previewComp.getTeam() : "Neutral";
        
        // Initialize immediately to prevent wrong context menu
        instance.Init(prefabToPlace, selectedItemType, objectName);
        instance.setTeam(team);
        
        // Ensure ItemType is correctly set (double-check)
        if (instance.ItemType != selectedItemType)
        {
            Debug.LogWarning($"[ObjectPlacer] ItemType mismatch detected. Setting {objectName} to {selectedItemType}");
            instance.ItemType = selectedItemType;
        }
        
        // Re-enable collider after initialization
        if (col != null) col.enabled = true;
        
        // Apply preserved attributes if relocating
        if (isRelocating && relocatingAttributes != null)
        {
            instance.SetProficiency(relocatingAttributes[0]);
            instance.SetFatigue(relocatingAttributes[1]);
            instance.SetCommsClarity(relocatingAttributes[2]);
            instance.SetEquipment(relocatingAttributes[3]);
            relocatingAttributes = null;
        }

        // Copy ViewRangeVisualizer settings from preview to placed object but don't show ring
        var placedVisualizer = placedObject.GetComponent<ViewRangeVisualizer>();
        
        if (placedVisualizer != null && selectedItemType == PlaceableItem.ItemType.Unit)
        {
            if (previewVisualizer != null && previewObject != null)
            {
                placedVisualizer.edgeSegmentPrefab = previewVisualizer.edgeSegmentPrefab;
                placedVisualizer.radius = previewVisualizer.radius;
                placedVisualizer.tileSize = previewVisualizer.tileSize;
            }
            // Explicitly clear any existing ring on placed object
            placedVisualizer.ClearRing();
        }

        // This block ensures the instance is always tracked
        if (selectedItemType == PlaceableItem.ItemType.Unit)
        {
            // Debug.Log($"[ObjectPlacer] Registering unit - Name: {objectName}, Team: {team}");
            placedUnits.Add(instance);
            // Debug.Log($"[ObjectPlacer] Total placed units: {placedUnits.Count}");
            
            OnUnitPlaced?.Invoke(instance);
            // Debug.Log($"[ObjectPlacer] OnUnitPlaced event invoked for {objectName}");

            if (teamList != null)
            {
                teamList.AddUnit(objectName, team);
                // Debug.Log($"[ObjectPlacer] Added unit to teamList: {objectName}");
            }
            else
            {
                // Debug.LogWarning($"[ObjectPlacer] teamList is null, cannot add unit {objectName}");
            }
        }
        else
        {
            // Debug.Log($"[ObjectPlacer] Placed item (not unit) - Name: {objectName}, ItemType: {selectedItemType}");
        }

        isRelocating = false;
        EndPlacement();
    }

    private IEnumerator ShowPreviewRingAfterPosition(ViewRangeVisualizer visualizer)
    {
        //Debug.Log($"[ObjectPlacer] ShowPreviewRingAfterPosition coroutine started for {visualizer?.gameObject.name}");
        yield return null; // Wait one frame for position to be set
        if (visualizer != null)
        {
            //Debug.Log($"[ObjectPlacer] Calling ShowRing on preview visualizer at position {visualizer.transform.position}");
            visualizer.ShowRing();
            yield return null; // Wait another frame for segments to be created
            // Debug.Log($"[ObjectPlacer] Updating ring position for preview visualizer");
            visualizer.UpdateRingPosition();
        }
        else
        {
            Debug.Log($"[ObjectPlacer] Visualizer is NULL in coroutine");
        }
    }

    public void RelocateUnit(PlaceableItemInstance unit)
    {
        // Preserve attributes before deletion
        relocatingAttributes = new int[] {
            unit.GetProficiency(),
            unit.GetFatigue(),
            unit.GetCommsClarity(),
            unit.GetEquipment()
        };
        
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

        bool isOverUI = EventSystem.current.IsPointerOverGameObject();
        if (isOverUI) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Vector3 position = hit.point;
            Renderer rend = previewObject.GetComponentInChildren<Renderer>(true);
            if (rend != null) position.y += rend.bounds.extents.y;
            previewObject.transform.position = position;
        }
    }

    private void EndPlacement()
    {
        // Debug.Log($"[ObjectPlacer] EndPlacement called - isPlacing was: {isPlacing}");
        isPlacing = false;

        if (previewObject != null)
        {
            // Clear any view range ring from preview object
            var previewVisualizer = previewObject.GetComponent<ViewRangeVisualizer>();
            if (previewVisualizer != null)
            {
                previewVisualizer.ClearRing();
            }
            
            // Debug.Log($"[ObjectPlacer] EndPlacement - Destroying preview object: {previewObject.name}");
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
        // Debug.Log($"[ObjectPlacer] AddUnit called for: {unit?.getName()}");
        
        if (!placedUnits.Contains(unit))
        {
            placedUnits.Add(unit);
            // Debug.Log($"[ObjectPlacer] Unit added to placedUnits. Total count: {placedUnits.Count}");
            
            if (teamList != null)
            {
                teamList.AddUnit(unit.getName(), unit.getTeam());
                // Debug.Log($"[ObjectPlacer] Unit added to teamList: {unit.getName()}");
            }
            else
            {
                // Debug.LogWarning($"[ObjectPlacer] teamList is null in AddUnit for {unit.getName()}");
            }
        }
        else
        {
            // Debug.Log($"[ObjectPlacer] Unit {unit?.getName()} already exists in placedUnits");
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