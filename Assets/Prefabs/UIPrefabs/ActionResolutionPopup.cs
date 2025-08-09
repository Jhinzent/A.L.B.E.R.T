using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ActionResolutionPopup : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text unitNameText;
    public TMP_Text teamNameText;
    public TMP_Text actionTypeText;
    public TMP_InputField actionDescriptionInput;
    public RadioButtonUI biasRadioButton; // CHANGED from TMP_Dropdown
    public TMP_Text resultText;

    public Button closeButton;
    public Button deleteButton;
    public Button rollButton;

    public event Action OnDeleteRequested;
    public event Action OnCloseRequested;

    // New event to notify when data changed (optional)
    public event Action<string, BiasType, string> OnDataChanged;

    private BiasType selectedBias;
    private System.Random rng = new System.Random();

    // Properties to get/set persisted data
    public string ActionDescription
    {
        get => actionDescriptionInput.text;
        set => actionDescriptionInput.text = value;
    }

    public BiasType SelectedBias
    {
        get => selectedBias;
        set
        {
            selectedBias = value;
            biasRadioButton.SelectOption(selectedBias.ToString());
        }
    }

    public string OutcomeText
    {
        get => resultText.text;
        set => resultText.text = value;
    }

    /// <summary>
    /// Initialize popup with fixed data and optionally pre-filled persisted values.
    /// </summary>
    public void Initialize(string unitName, string teamName, string actionType, string actionDescription,
                           BiasType? bias = null, string outcome = "")
    {
        unitNameText.text = unitName;
        teamNameText.text = teamName;
        actionTypeText.text = actionType;

        SetupBiasRadioButton();

        // Set persisted or default values
        ActionDescription = actionDescription;
        SelectedBias = bias ?? BiasType.Neutral;
        OutcomeText = outcome;

        // Listen for UI changes to update persisted data via event
        actionDescriptionInput.onValueChanged.RemoveAllListeners();
        actionDescriptionInput.onValueChanged.AddListener(OnInputChanged);

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => OnCloseRequested?.Invoke());

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => OnDeleteRequested?.Invoke());

        rollButton.onClick.RemoveAllListeners();
        rollButton.onClick.AddListener(RollOutcome);
    }

    private void OnInputChanged(string _)
    {
        OnDataChanged?.Invoke(ActionDescription, SelectedBias, OutcomeText);
    }

    private void SetupBiasRadioButton()
    {
        if (biasRadioButton != null)
        {
            string[] biasOptions = Enum.GetNames(typeof(BiasType));
            biasRadioButton.SetOptions(biasOptions);

            biasRadioButton.OnOptionSelected -= OnBiasOptionSelected;
            biasRadioButton.OnOptionSelected += OnBiasOptionSelected;

            biasRadioButton.SelectOption(selectedBias.ToString());
        }
    }

    private void OnBiasOptionSelected(string selected)
    {
        if (Enum.TryParse<BiasType>(selected, out var parsedBias))
        {
            selectedBias = parsedBias;
            OnDataChanged?.Invoke(ActionDescription, selectedBias, OutcomeText);
        }
        else
        {
            Debug.LogWarning($"Unknown bias type selected: {selected}");
        }
    }

    private void RollOutcome()
    {
        StartCoroutine(DisplayLoadingThenResult());
    }

    private System.Collections.IEnumerator DisplayLoadingThenResult()
    {
        string[] loadingSteps = { "X", "O", "X" };
        float delay = 0.4f;

        foreach (var step in loadingSteps)
        {
            resultText.text = step;
            yield return new WaitForSeconds(delay);
        }

        int roll = RollBiasedD6(selectedBias);
        OutcomeTier outcome = EvaluateOutcome(roll);
        resultText.text = $"{outcome}";

        // Notify data changed (new outcome)
        OnDataChanged?.Invoke(ActionDescription, selectedBias, OutcomeText);
    }

    public void SetBias(BiasType bias)
    {
        selectedBias = bias;
        biasRadioButton.SelectOption(bias.ToString());
    }

    public BiasType GetSelectedBias()
    {
        return selectedBias;
    }

    public void SetOutcome(string outcome)
    {
        resultText.text = outcome;
    }

    public string GetOutcome()
    {
        return resultText.text;
    }

    public string GetActionDescription()
    {
        return actionDescriptionInput.text;
    }

    private int RollBiasedD6(BiasType bias)
    {
        float[] weights = bias switch
        {
            BiasType.VeryUnfavorable => new float[] { 0.4f, 0.3f, 0.15f, 0.1f, 0.04f, 0.01f },
            BiasType.Unfavorable => new float[] { 0.3f, 0.25f, 0.2f, 0.15f, 0.07f, 0.03f },
            BiasType.Neutral => new float[] { 1, 1, 1, 1, 1, 1 },
            BiasType.Favorable => new float[] { 0.03f, 0.07f, 0.15f, 0.2f, 0.25f, 0.3f },
            BiasType.VeryFavorable => new float[] { 0.01f, 0.04f, 0.1f, 0.15f, 0.3f, 0.4f },
            _ => new float[] { 1, 1, 1, 1, 1, 1 },
        };

        float total = 0f;
        foreach (float w in weights) total += w;
        float rand = (float)rng.NextDouble() * total;

        float cumulative = 0f;
        for (int i = 0; i < 6; i++)
        {
            cumulative += weights[i];
            if (rand <= cumulative)
                return i + 1;
        }

        return 6;
    }


    private OutcomeTier EvaluateOutcome(int roll)
    {
        return roll switch
        {
            1 => OutcomeTier.CriticalFail,
            2 => OutcomeTier.Fail,
            3 or 4 => OutcomeTier.Mixed,
            5 => OutcomeTier.Success,
            6 => OutcomeTier.CriticalSuccess,
            _ => OutcomeTier.Mixed,
        };
    }

    public enum BiasType
    {
        VeryUnfavorable,
        Unfavorable,
        Neutral,
        Favorable,
        VeryFavorable
    }

    public enum OutcomeTier
    {
        CriticalFail,
        Fail,
        Mixed,
        Success,
        CriticalSuccess
    }
}