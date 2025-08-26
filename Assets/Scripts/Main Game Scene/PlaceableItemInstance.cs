using UnityEngine;

public class PlaceableItemInstance : MonoBehaviour
{
    public GameObject OriginalPrefab;
    public string team;

    // NEW ATTRIBUTES (persistent)
    public int proficiency = 1;
    public int fatigue = 1;
    public int commsClarity = 1;
    public int equipment = 1;

    private string itemName;

    public PlaceableItem.ItemType ItemType { get; private set; }
    public event System.Action<PlaceableItemInstance> OnDestroyed;
    private GameObject teamIndicator;

    private static readonly Color[] teamColors = new Color[]
    {
        Color.red, Color.blue, Color.green, Color.yellow, Color.magenta,
        Color.cyan, new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 1f),
        new Color(0.5f, 0.5f, 0.5f), new Color(0f, 0.75f, 0.75f)
    };

    private static readonly Color neutralColor = Color.white;

    public void Init(GameObject prefab, PlaceableItem.ItemType type, string initName)
    {
        OriginalPrefab = prefab;
        ItemType = type;
        team = "Neutral";

        // Default values for new attributes
        proficiency = 1;
        fatigue = 1;
        commsClarity = 1;
        equipment = 1;

        itemName = initName;

        CreateTeamIndicator();
    }

    public string getTeam() => team;
    public void setTeam(string newTeam)
    {
        team = newTeam;
        UpdateTeamIndicatorColor();
    }

    // Getters/Setters for new attributes
    // Getters/Setters
    public int GetProficiency() => proficiency;
    public void SetProficiency(int v) => proficiency = (int)Mathf.Clamp(v, 1, 5);

    public int GetFatigue() => fatigue;
    public void SetFatigue(int v) => fatigue = (int)Mathf.Clamp(v, 1, 5);

    public int GetCommsClarity() => commsClarity;
    public void SetCommsClarity(int v) => commsClarity = (int)Mathf.Clamp(v, 1, 5);

    public int GetEquipment() => equipment;
    public void SetEquipment(int v) => equipment = (int)Mathf.Clamp(v, 1, 5);

    public void SetName(string newName) => itemName = newName;
    public string getName() => itemName;
    public GameObject getOriginalPrefab() => OriginalPrefab;
    public bool IsUnit() => ItemType == PlaceableItem.ItemType.Unit;
    public bool IsObject() => ItemType == PlaceableItem.ItemType.Object;

    public void OnClicked()
    {
        Debug.Log($"[PlaceableItemInstance] OnClicked called for {itemName}");
        ContextMenuManager.Instance.ShowContextMenu(this, transform.position);
    }

    private void OnMouseDown()
    {
        Debug.Log($"[PlaceableItemInstance] OnMouseDown detected for {itemName}");
        OnClicked();
    }

    public void Delete()
    {
        if (IsUnit() && ObjectPlacer.Instance != null)
        {
            ObjectPlacer.Instance.RemoveUnit(this); // Handles both internal list and TeamList
        }

        OnDestroyed?.Invoke(this);
        Destroy(gameObject);
    }

    private void CreateTeamIndicator()
    {
        if (!IsUnit()) return;

        teamIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        teamIndicator.name = "TeamIndicator";
        Destroy(teamIndicator.GetComponent<Collider>());

        teamIndicator.transform.SetParent(transform);
        teamIndicator.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        teamIndicator.transform.localPosition = new Vector3(0, 2.5f, 0);

        UpdateTeamIndicatorColor();
    }

    private void UpdateTeamIndicatorColor()
    {
        if (teamIndicator == null) return;

        Color color = neutralColor;

        if (team.StartsWith("Player "))
        {
            if (int.TryParse(team.Substring(7), out int teamNumber))
            {
                if (teamNumber >= 1 && teamNumber <= teamColors.Length)
                {
                    color = teamColors[teamNumber - 1];
                }
            }
        }

        var renderer = teamIndicator.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Unlit/Color"));
            renderer.material.color = color;
        }
    }
}