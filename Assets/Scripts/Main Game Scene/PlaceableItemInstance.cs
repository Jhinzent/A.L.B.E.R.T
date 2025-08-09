using UnityEngine;

public class PlaceableItemInstance : MonoBehaviour
{
    public GameObject OriginalPrefab;
    public string team;
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
        itemName = initName;

        CreateTeamIndicator();
    }

    public string getTeam()
    {
        return team;
    }

    public void setTeam(string newTeam)
    {
        team = newTeam;
        UpdateTeamIndicatorColor();
    }

    public void SetName(string newName)
    {
        itemName = newName;
    }

    public string getName()
    {
        return itemName;
    }

    public GameObject getOriginalPrefab()
    {
        return OriginalPrefab;
    }

    public bool IsUnit()
    {
        return ItemType == PlaceableItem.ItemType.Unit;
    }

    public bool IsObject()
    {
        return ItemType == PlaceableItem.ItemType.Object;
    }

    public void OnClicked()
    {
        ContextMenuManager.Instance.ShowContextMenu(this, transform.position);
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