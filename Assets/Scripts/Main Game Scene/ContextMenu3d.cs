using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ContextMenu3D : MonoBehaviour
{
    private PlaceableItemInstance target;
    public GameObject subPanel;
    public TMP_Text titleText;
    public TMP_InputField titleInputField;
    private bool isEditingTitle = false;
    public Image teamColorImage;
    public RadioButtonUI teamChangeRadioButton;
    public RadioButtonUI actionTypeRadioButton;

    // === Attribute UI References ===
    [Header("Attribute Buttons (1–5 each)")]
    public Button[] proficiencyButtons; // How adept they are
    public Button[] fatigueButtons;     // How exhausted they are
    public Button[] commsClarityButtons; // Communication reachability
    public Button[] equipmentButtons;   // How well equipped they are

    // Attribute Values
    private int proficiencyValue = 1;
    private int fatigueValue = 1;
    private int connectivityValue = 1;
    private int equipmentValue = 1;

    private Color activeColor = Color.white;
    private Color inactiveColor = Color.gray;

    private void Start()
    {
        if (titleInputField != null)
        {
            titleInputField.gameObject.SetActive(false);
            titleInputField.onEndEdit.AddListener(OnTitleEditEnd);
        }

        if (titleText != null)
        {
            var titleTextButton = titleText.gameObject.GetComponent<Button>();
            if (titleTextButton == null)
                titleTextButton = titleText.gameObject.AddComponent<Button>();

            titleTextButton.onClick.AddListener(BeginEditTitle);
        }

        if (actionTypeRadioButton != null)
        {
            actionTypeRadioButton.SetOptions("Action", "Movement", "Combat", "Investigate", "Watch", "Custom");
            actionTypeRadioButton.OnOptionSelected -= OnActionOptionSelected;
            actionTypeRadioButton.OnOptionSelected += OnActionOptionSelected;
        }

        // Hook up attribute button events
        InitAttributeButtons(proficiencyButtons, SetProficiency);
        InitAttributeButtons(fatigueButtons, SetFatigue);
        InitAttributeButtons(commsClarityButtons, SetConnectivity);
        InitAttributeButtons(equipmentButtons, SetEquipment);

        // Set default values
        UpdateAttributeUI(proficiencyButtons, proficiencyValue);
        UpdateAttributeUI(fatigueButtons, fatigueValue);
        UpdateAttributeUI(commsClarityButtons, connectivityValue);
        UpdateAttributeUI(equipmentButtons, equipmentValue);
    }

    private void InitAttributeButtons(Button[] buttons, System.Action<int> callback)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i + 1; // Values go 1–5
            buttons[i].onClick.AddListener(() => callback(index));
        }
    }

    private void UpdateAttributeUI(Button[] buttons, int value)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            var colors = buttons[i].colors;
            colors.normalColor = (i < value) ? activeColor : inactiveColor;
            colors.highlightedColor = colors.normalColor;
            buttons[i].colors = colors;
        }
    }

    public void SetProficiency(int value)
    {
        proficiencyValue = value;
        UpdateAttributeUI(proficiencyButtons, value);
        if (target != null) target.SetProficiency(value);
    }

    public void SetFatigue(int value)
    {
        fatigueValue = value;
        UpdateAttributeUI(fatigueButtons, value);
        if (target != null) target.SetFatigue(value);
    }

    public void SetConnectivity(int value)
    {
        connectivityValue = value;
        UpdateAttributeUI(commsClarityButtons, value);
        if (target != null) target.SetCommsClarity(value);
    }

    public void SetEquipment(int value)
    {
        equipmentValue = value;
        UpdateAttributeUI(equipmentButtons, value);
        if (target != null) target.SetEquipment(value);
    }

    private void LoadAttributesFromTarget()
    {
        if (target == null) return;

        proficiencyValue = target.GetProficiency();
        fatigueValue = target.GetFatigue();
        connectivityValue = target.GetCommsClarity();
        equipmentValue = target.GetEquipment();

        UpdateAttributeUI(proficiencyButtons, proficiencyValue);
        UpdateAttributeUI(fatigueButtons, fatigueValue);
        UpdateAttributeUI(commsClarityButtons, connectivityValue);
        UpdateAttributeUI(equipmentButtons, equipmentValue);
    }
    
    private System.Collections.IEnumerator ShowRotationButtonsDelayed()
    {
        yield return null;
        if (target != null)
        {
            var rotationComponent = target.GetComponent<PlaceableItemRotation>();
            if (rotationComponent != null)
                rotationComponent.ShowButtons();
        }
    }

    // === Your existing methods unchanged ===
    private static readonly Color[] teamColors = new Color[]
    {
        Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan,
        new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 1f), new Color(0.5f, 0.5f, 0.5f), new Color(0f, 0.75f, 0.75f)
    };
    private static readonly Color neutralColor = new Color32(255, 255, 255, 255);
    public PlaceableItemInstance Item => target;

    public bool IsVisible()
    {
        return gameObject.activeInHierarchy;
    }

    public void Init(PlaceableItemInstance targetObject)
    {
        target = targetObject;
        subPanel.SetActive(false);

        if (teamChangeRadioButton != null)
        {
            teamChangeRadioButton.SetOptions("Neutral", "Player 1", "Player 2", "Player 3", "Player 4");
            teamChangeRadioButton.OnOptionSelected -= SetTeamForItemInstance;
            teamChangeRadioButton.OnOptionSelected += SetTeamForItemInstance;

            if (target != null)
            {
                string teamName = target.getTeam();
                if (!string.IsNullOrEmpty(teamName))
                {
                    teamChangeRadioButton.SelectOption(teamName);
                }
            }
        }

        if (target != null)
        {
            string teamName = target.getTeam();

            if (teamColorImage != null)
            {
                if (teamName == "Neutral")
                {
                    teamColorImage.color = neutralColor;
                }
                else if (teamName.StartsWith("Player "))
                {
                    if (int.TryParse(teamName.Substring(7), out int teamNumber))
                    {
                        if (teamNumber >= 1 && teamNumber <= teamColors.Length)
                        {
                            teamColorImage.color = teamColors[teamNumber - 1];
                        }
                    }
                }
            }

            if (titleText != null)
            {
                titleText.text = target.getName();
            }

            // Load attributes from PlaceableItemInstance
            LoadAttributesFromTarget();
            
            // Show rotation buttons for this target after a frame delay
            StartCoroutine(ShowRotationButtonsDelayed());
        }
    }

    private void BeginEditTitle()
    {
        if (isEditingTitle) return;
        isEditingTitle = true;
        if (titleText != null && titleInputField != null)
        {
            titleInputField.text = titleText.text;
            titleText.gameObject.SetActive(false);
            titleInputField.gameObject.SetActive(true);
            titleInputField.Select();
            titleInputField.ActivateInputField();
        }
    }

    private void OnTitleEditEnd(string newText)
    {
        if (!isEditingTitle) return;
        isEditingTitle = false;
        if (titleText != null && titleInputField != null)
        {
            titleText.text = newText;
            titleText.gameObject.SetActive(true);
            titleInputField.gameObject.SetActive(false);

            if (target != null)
            {
                string oldName = target.getName();
                target.SetName(newText);
                
                string teamName = target.getTeam();
                if (!string.IsNullOrEmpty(teamName))
                {
                    TeamList teamList = ContextMenuManager.Instance.TeamList;
                    if (teamList != null)
                    {
                        teamList.RemoveUnit(oldName);
                        teamList.AddUnit(newText, teamName);
                    }
                    ActionScrollViewManager actionScrollView = ContextMenuManager.Instance.ActionScrollView;
                    if (actionScrollView != null)
                    {
                        actionScrollView.UpdateUnitName(target, newText);
                    }
                }
            }
        }
    }

    private void OnActionOptionSelected(string option)
    {
        if (target != null)
        {
            var actionScrollViewManager = ContextMenuManager.Instance.ActionScrollView;
            if (actionScrollViewManager != null)
            {
                actionScrollViewManager.LogAction(target, option);
                if ((option != null) && (option == "Movement"))
                {
                    OnStartMovementPathClicked();
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (teamChangeRadioButton != null)
        {
            teamChangeRadioButton.OnOptionSelected -= SetTeamForItemInstance;
        }
        if (actionTypeRadioButton != null)
        {
            actionTypeRadioButton.OnOptionSelected -= OnActionOptionSelected;
        }
        
        // Hide rotation buttons when context menu is destroyed
        if (target != null)
        {
            var rotationComponent = target.GetComponent<PlaceableItemRotation>();
            if (rotationComponent != null)
                rotationComponent.HideButtons();
        }
    }

    public void OnRelocateClicked()
    {
        if (target != null)
        {
            ObjectPlacer.Instance.RelocateUnit(target);
        }
        ContextMenuManager.Instance.HideContextMenu();
    }
    public void OnToggleSubmenuClicked()
    {
        bool newState = !subPanel.activeSelf;
        subPanel.SetActive(newState);
    }
    public void OnDeleteClicked()
    {
        if (target != null)
            target.Delete();
        ContextMenuManager.Instance.HideContextMenu();
    }
    public void OnCloseClicked()
    {
        ContextMenuManager.Instance.HideContextMenu();
    }
    public void OnStartMovementPathClicked()
    {
        if (target != null)
        {
            var movement = target.GetComponent<UnitMovement>();
            if (movement != null)
            {
                movement.EnterMovementMode();
            }
        }
    }
    
    public void OnToggleViewRangeClicked()
    {
        if (target != null)
        {
            var visualizer = target.GetComponent<ViewRangeVisualizer>();
            if (visualizer != null)
            {
                if (visualizer.IsRingVisible())
                    visualizer.ClearRing();
                else
                    visualizer.ShowRing();
            }
        }
    }
    public void SetTeamForItemInstance(string teamName)
    {
        if (target == null || string.IsNullOrEmpty(teamName)) return;
        string previousName = target.getName();
        target.setTeam(teamName);
        if (teamColorImage != null)
        {
            if (teamName == "Neutral")
            {
                teamColorImage.color = neutralColor;
            }
            else if (teamName.StartsWith("Player "))
            {
                if (int.TryParse(teamName.Substring(7), out int teamNumber))
                {
                    if (teamNumber >= 1 && teamNumber <= teamColors.Length)
                    {
                        teamColorImage.color = teamColors[teamNumber - 1];
                    }
                }
            }
        }
        TeamList teamList = ContextMenuManager.Instance.TeamList;
        if (teamList != null)
        {
            teamList.RemoveUnit(previousName);
            teamList.AddUnit(previousName, teamName);
        }
    }
}