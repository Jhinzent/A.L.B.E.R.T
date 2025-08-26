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
                // Debug.Log($"[ObjectPlacer] Left mouse button pressed - calling PlaceObject()");
                PlaceObject();
            }
            if (Input.GetMouseButtonDown(1))
            {
                // Debug.Log($"[ObjectPlacer] Right mouse button pressed - calling EndPlacement()");
                EndPlacement();
            }
        }
    }

    public void SetSelectedPrefab(GameObject prefab, PlaceableItem.ItemType itemType, string existingName = null, string existingTeam = "Neutral")
    {
        // Debug.Log($"[ObjectPlacer] === SetSelectedPrefab START === Prefab: {prefab?.name}, ItemType: {itemType}, Name: {existingName}, Team: {existingTeam}");

        selectedPrefab = prefab;
        selectedItemType = itemType;

        if (previewObject != null)
        {
            //Debug.Log($"[ObjectPlacer] Destroying previous preview object: {previewObject.name}");
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
        // Debug.Log($"[ObjectPlacer] PlaceableItemInstance component: {(instance != null ? "Found/Added" : "NULL")}");
        
        instance.Init(prefab, itemType, existingName ?? "NewObject");
        instance.setTeam(existingTeam);
        // Debug.Log($"[ObjectPlacer] Preview instance initialized - Name: {instance.getName()}, Team: {instance.getTeam()}, ItemType: {instance.ItemType}");
        // Debug.Log($"[ObjectPlacer] === SetSelectedPrefab END === isPlacing: {isPlacing}");

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
        if (previewObject == null || selectedPrefab == null)
        {
            Debug.LogWarning($"[ObjectPlacer] PlaceObject failed - previewObject: {previewObject != null}, selectedPrefab: {selectedPrefab != null}");
            return;
        }

        // Debug.Log($"[ObjectPlacer] PlaceObject called - placing {selectedPrefab.name} at {previewObject.transform.position}");
        
        GameObject placedObject = Instantiate(selectedPrefab, previewObject.transform.position, previewObject.transform.rotation);
        var previewInstance = previewObject.GetComponent<PlaceableItemInstance>();
        var instance = placedObject.GetComponent<PlaceableItemInstance>() ?? placedObject.AddComponent<PlaceableItemInstance>();

        string objectName = previewInstance != null ? previewInstance.getName() : "NewObject";
        string team = previewInstance != null ? previewInstance.getTeam() : "Neutral";
        
        // Debug.Log($"[ObjectPlacer] Placed object instance: {(instance != null ? "Found/Added" : "NULL")}");
        // Debug.Log($"[ObjectPlacer] Object details - Name: {objectName}, Team: {team}, ItemType: {selectedItemType}");

        instance.Init(selectedPrefab, selectedItemType, objectName);
        instance.setTeam(team);
        
        // Apply preserved attributes if relocating
        if (isRelocating && relocatingAttributes != null)
        {
            instance.SetProficiency(relocatingAttributes[0]);
            instance.SetFatigue(relocatingAttributes[1]);
            instance.SetCommsClarity(relocatingAttributes[2]);
            instance.SetEquipment(relocatingAttributes[3]);
            relocatingAttributes = null;
        }

        // Copy ViewRangeVisualizer settings from preview to placed object
        var previewVisualizer = previewObject.GetComponent<ViewRangeVisualizer>();
        var placedVisualizer = placedObject.GetComponent<ViewRangeVisualizer>();
        
        //Debug.Log($"Preview visualizer: {(previewVisualizer != null ? "Found" : "NULL")}");
        // Debug.Log($"Placed visualizer: {(placedVisualizer != null ? "Found" : "NULL")}");
        
        if (placedVisualizer != null && selectedItemType == PlaceableItem.ItemType.Unit)
        {
            if (previewVisualizer != null)
            {
                placedVisualizer.edgeSegmentPrefab = previewVisualizer.edgeSegmentPrefab;
                placedVisualizer.radius = previewVisualizer.radius;
                placedVisualizer.tileSize = previewVisualizer.tileSize;
            }
            // Show ring immediately like ContextMenuManager does
            placedVisualizer.ShowRing();
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
        if (previewObject == null)
        {
            Debug.LogWarning($"[ObjectPlacer] MovePreviewToMousePosition - previewObject is null!");
            return;
        }
        if (Camera.main == null)
        {
            Debug.LogWarning($"[ObjectPlacer] MovePreviewToMousePosition - Camera.main is null!");
            return;
        }

        bool isOverUI = EventSystem.current.IsPointerOverGameObject();
        if (isOverUI)
        {
            // Debug.Log($"[ObjectPlacer] Mouse is over UI element, skipping position update");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Vector3 position = hit.point;
            Renderer rend = previewObject.GetComponentInChildren<Renderer>(true);
            if (rend != null) position.y += rend.bounds.extents.y;
            previewObject.transform.position = position;
        }
        else
        {
            Debug.LogWarning($"[ObjectPlacer] Raycast failed to hit ground layer. LayerMask: {groundLayer.value}");
        }
    }

    private void EndPlacement()
    {
        // Debug.Log($"[ObjectPlacer] EndPlacement called - isPlacing was: {isPlacing}");
        isPlacing = false;

        if (previewObject != null)
        {
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