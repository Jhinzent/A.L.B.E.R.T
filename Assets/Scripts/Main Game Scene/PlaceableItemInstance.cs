using UnityEngine;
using TMPro;

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
    private TextMeshPro nameText;
    private Material backgroundMaterial;
    private Transform backgroundTransform;

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

    public void SetName(string newName)
    {
        itemName = newName;
        if (nameText != null)
        {
            nameText.text = itemName;
            UpdateBackgroundSize();
        }
    }
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

    private void Update()
    {
        if (teamIndicator != null)
        {
            teamIndicator.transform.position = transform.TransformPoint(new Vector3(0, 2.5f, 0));
        }
    }

    private void OnDestroy()
    {
        if (teamIndicator != null) Destroy(teamIndicator);
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

        teamIndicator = new GameObject("NameIndicator");
        teamIndicator.transform.position = transform.TransformPoint(new Vector3(0, 2.5f, 0));
        teamIndicator.transform.rotation = Quaternion.Euler(45f, 0f, 0f);

        // Create background panel with padding
        var background = GameObject.CreatePrimitive(PrimitiveType.Quad);
        background.name = "Background";
        background.transform.SetParent(teamIndicator.transform);
        background.transform.localPosition = new Vector3(0, 0, -0.1f);
        background.transform.localRotation = Quaternion.identity;
        backgroundTransform = background.transform;
        Destroy(background.GetComponent<Collider>());
        
        var bgRenderer = background.GetComponent<Renderer>();
        backgroundMaterial = new Material(Shader.Find("Standard"));
        backgroundMaterial.SetFloat("_Mode", 3);
        backgroundMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        backgroundMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        backgroundMaterial.SetInt("_ZWrite", 0);
        backgroundMaterial.DisableKeyword("_ALPHATEST_ON");
        backgroundMaterial.EnableKeyword("_ALPHABLEND_ON");
        backgroundMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        backgroundMaterial.renderQueue = 3000;
        bgRenderer.material = backgroundMaterial;

        nameText = teamIndicator.AddComponent<TextMeshPro>();
        nameText.text = itemName;
        nameText.fontSize = 20f;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.fontStyle = FontStyles.Bold;
        nameText.fontMaterial.EnableKeyword("OUTLINE_ON");
        nameText.outlineWidth = 0.25f;
        nameText.outlineColor = Color.black;
        nameText.sortingOrder = 1;

        UpdateBackgroundSize();
        UpdateTeamIndicatorColor();
    }

    private void UpdateTeamIndicatorColor()
    {
        if (nameText == null) return;

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

        nameText.color = color;
        if (backgroundMaterial != null)
        {
            backgroundMaterial.color = new Color(color.r, color.g, color.b, 0.35f);
        }
    }

    private void UpdateBackgroundSize()
    {
        if (nameText != null && backgroundTransform != null)
        {
            nameText.ForceMeshUpdate();
            float textWidth = nameText.preferredWidth;
            float textHeight = nameText.preferredHeight;
            backgroundTransform.localScale = new Vector3(textWidth + 2f, textHeight + 1f, 1f);
        }
    }
}