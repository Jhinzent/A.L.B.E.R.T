using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

public class RadioButtonUI : MonoBehaviour
{
    [Header("Setup")]
    public GameObject optionButtonPrefab;
    public TMP_Text mainButtonText;
    public float yOffsetPerItem = 30f;
    public bool use3DRotation = false;
    public List<string> options = new List<string>();
    public event Action<string> OnOptionSelected;
    private Button mainButton;
    private bool isOpen = false;
    private List<GameObject> spawnedButtons = new List<GameObject>();

    void Awake()
    {
        mainButton = GetComponent<Button>();

        if (mainButton == null)
            Debug.LogError("RadioButtonUI: Missing Button component.");
        if (mainButtonText == null)
            Debug.LogError("RadioButtonUI: Missing TMP_Text.");
        if (optionButtonPrefab == null)
            Debug.LogError("RadioButtonUI: optionButtonPrefab not assigned.");
    }

    void Start()
    {
        if (mainButton == null || mainButtonText == null || optionButtonPrefab == null)
            return;

        mainButton.onClick.AddListener(ToggleDropdown);
    }

    public void SelectOption(string option)
    {
        if (options.Contains(option))
        {
            mainButtonText.text = option;
        }
        else
        {
            Debug.LogWarning($"RadioButtonUI: Option '{option}' not found in list.");
        }
    }

    public string GetSelectedOption() => mainButtonText != null ? mainButtonText.text : null;

    public void SetOptions(params string[] newOptions)
    {
        CloseDropdown();
        options = new List<string>(newOptions);
        if (options.Count > 0)
        {
            mainButtonText.text = options[0];
        }
    }

    void ToggleDropdown()
    {
        if (isOpen) CloseDropdown();
        else OpenDropdown();
    }

    void OpenDropdown()
    {
        if (options.Count <= 1) return;

        RectTransform mainRect = GetComponent<RectTransform>();
        Vector3 basePosition = mainRect.position;

        string currentSelection = mainButtonText.text;
        int index = 0;

        for (int i = 0; i < options.Count; i++)
        {
            string value = options[i];
            if (value == currentSelection)
                continue;

            GameObject optionGO = Instantiate(optionButtonPrefab, transform.parent);
            RectTransform optionRect = optionGO.GetComponent<RectTransform>();
            optionRect.sizeDelta = mainRect.sizeDelta;

            float yOffset = yOffsetPerItem * (index + 1);
            Vector3 newPos;

            if (use3DRotation)
            {
                // For a 40° tilt around X, calculate z offset
                float angleInRad = 40f * Mathf.Deg2Rad;
                float zOffset = Mathf.Sin(angleInRad) * yOffset;
                float adjustedY = basePosition.y - Mathf.Cos(angleInRad) * yOffset;

                newPos = new Vector3(basePosition.x, adjustedY, basePosition.z - zOffset);
            }
            else
            {
                newPos = basePosition - new Vector3(0, yOffset, 0);
            }

            optionRect.position = newPos;

            TMP_Text txt = optionGO.GetComponentInChildren<TMP_Text>();
            if (txt != null)
                txt.text = value;

            Button optionBtn = optionGO.GetComponent<Button>();
            if (optionBtn != null)
            {
                optionBtn.onClick.AddListener(() =>
                {
                    // Debug.Log($"RadioButtonUI: Option '{value}' clicked.");
                    mainButtonText.text = value;
                    OnOptionSelected?.Invoke(value);
                    CloseDropdown();
                });
            }

            spawnedButtons.Add(optionGO);
            index++;
        }

        isOpen = true;
    }

    void CloseDropdown()
    {
        foreach (var go in spawnedButtons)
        {
            Destroy(go);
        }
        spawnedButtons.Clear();
        isOpen = false;
    }
}