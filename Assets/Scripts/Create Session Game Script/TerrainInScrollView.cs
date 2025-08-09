using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TerrainInScrollView : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public Image colorPreview;
    public Button buttonComponent;

    public void SetTextComponent(string text)
    {
        if (textComponent != null)
        {
            textComponent.text = text;
        }
    }

    public void SetColorComponent(Color color)
    {
        if (colorPreview != null)
        {
            colorPreview.color = color;
        }
    }

    public Button GetButtonComponent()
    {
        return buttonComponent;
    }
}