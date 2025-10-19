using UnityEngine;
using UnityEngine.UI;

public class DrawColorButton : MonoBehaviour
{
    private Color color;
    private Button button;
    public Image image;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void SetColor(Color c)
    {
        color = c;
        if (image != null)
        {
            image.color = color;
        }
    }

    private void OnClick()
    {
        if (DrawMeshFull.Instance != null)
            DrawMeshFull.Instance.ActivateDrawing(color);
    }
}