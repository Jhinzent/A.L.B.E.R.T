using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContextMenu3D : MonoBehaviour
{
    private PlaceableItemInstance target;
    public GameObject subPanel;
    public TMP_Text titleText;
    public TMP_InputField titleInputField;
    private bool isEditingTitle = false;
    public Image teamColorImage;
    public RadioButtonUI teamChangeRadioButton;
    public RadioButtonUI actionTypeRadioButton;  // NEW

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
    }

    private static readonly Color[] teamColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta,
        Color.cyan,
        new Color(1f, 0.5f, 0f),
        new Color(0.5f, 0f, 1f),
        new Color(0.5f, 0.5f, 0.5f),
        new Color(0f, 0.75f, 0.75f)
    };

    private static readonly Color neutralColor = new Color32(255, 255, 255, 255);

    public PlaceableItemInstance Item => target;

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
                // Debug.Log($"[ContextMenu3D] Set title text to: {titleText.text}");
            }
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
            // Debug.Log("[ContextMenu3D] Begin editing title.");
        }
    }

    private void OnTitleEditEnd(string newText)
    {
        if (!isEditingTitle)
        {
            Debug.Log("[ContextMenu3D] Not editing title, ignoring end edit.");
            return;
        }

        isEditingTitle = false;

        if (titleText != null && titleInputField != null)
        {
            titleText.text = newText;
            titleText.gameObject.SetActive(true);
            titleInputField.gameObject.SetActive(false);

            if (target != null)
            {
                string oldName = target.getName(); // Save old name before change

                target.SetName(newText); // Change name on target

                // === Update TeamList UI ===
                TeamList teamList = ContextMenuManager.Instance.TeamList;
                if (teamList != null)
                {
                    string teamName = target.getTeam();
                    teamList.RemoveUnit(oldName);
                    teamList.AddUnit(newText, teamName);
                }
                else
                {
                    Debug.LogWarning("[ContextMenu3D] No reference to TeamList found.");
                }

                // === Update ActionScrollView ===
                ActionScrollViewManager actionScrollView = ContextMenuManager.Instance.ActionScrollView;
                if (actionScrollView != null)
                {
                    actionScrollView.UpdateUnitName(target, newText);
                }
                else
                {
                    Debug.LogWarning("[ContextMenu3D] No reference to ActionScrollViewManager found.");
                }
            }
            else
            {
                Debug.LogWarning("[ContextMenu3D] Target is null, can't save name.");
            }
        }
    }

    private void OnActionOptionSelected(string option)
    {
        // Debug.Log("[ContextMenu3D] Action selected: " + option);

        if (target != null)
        {
            // Log the action in the scroll view
            var actionScrollViewManager = ContextMenuManager.Instance.ActionScrollView;
            if (actionScrollViewManager != null)
            {
                actionScrollViewManager.LogAction(target, option);
                if ((option != null) && (option == "Movement")) {
                    OnStartMovementPathClicked();
                }
            }
            else
            {
                Debug.LogWarning("[ContextMenu3D] No reference to ActionScrollViewManager found.");
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
    }

    public void OnRelocateClicked()
    {
        if (target != null)
        {
            ObjectPlacer.Instance.RelocateUnit(target); // <- just this
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

    public void SetTeamForItemInstance(string teamName)
    {
        if (target == null || string.IsNullOrEmpty(teamName))
            return;

        string previousName = target.getName();

        // Update the model
        target.setTeam(teamName);

        // Update the UI team color indicator
        if (teamColorImage != null)
        {
            if (teamName == "Neutral")
            {
                teamColorImage.color = neutralColor;
                // Debug.Log("[ContextMenu3D] Set color to Neutral");
            }
            else if (teamName.StartsWith("Player "))
            {
                if (int.TryParse(teamName.Substring(7), out int teamNumber))
                {
                    if (teamNumber >= 1 && teamNumber <= teamColors.Length)
                    {
                        teamColorImage.color = teamColors[teamNumber - 1];
                        // Debug.Log($"[ContextMenu3D] Set color to {teamColors[teamNumber - 1]}");
                    }
                }
            }
        }

        // Update TeamList UI
        TeamList teamList = ContextMenuManager.Instance.TeamList;
        if (teamList != null)
        {
            teamList.RemoveUnit(previousName);
            teamList.AddUnit(previousName, teamName);
        }
        else
        {
            // Debug.LogWarning("[ContextMenu3D] No reference to TeamList found.");
        }
    }
}