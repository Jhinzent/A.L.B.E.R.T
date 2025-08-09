using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorScrollPopulator : MonoBehaviour
{
    [SerializeField] private Transform contentParent; // ScrollView/Viewport/Content
    [SerializeField] private GameObject colorButtonPrefab;
    [SerializeField] private GameObject scrollViewRoot; // Root GameObject of the scroll view (to hide/show it)
    [SerializeField] private GameObject deleteAllButtonPrefab;

    private Color[] colors = new Color[]
    {
        Color.black, Color.white, Color.red, Color.green, Color.blue,
        Color.yellow, Color.cyan, Color.magenta, new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 1f) // some extras
    };

    private bool isVisible = true;

    private void Start()
    {
        foreach (Color color in colors)
        {
            GameObject buttonGO = Instantiate(colorButtonPrefab, contentParent);
            DrawColorButton drawColorButton = buttonGO.GetComponent<DrawColorButton>();
            drawColorButton.SetColor(color);
        }

        GameObject deleteButtonGO = Instantiate(deleteAllButtonPrefab, contentParent);
        Button deleteButton = deleteButtonGO.GetComponent<Button>();
        deleteButton.onClick.AddListener(() =>
        {
            DrawMeshFull.Instance.ClearAllDrawings();
        });

        // Optional: Start with hidden scroll view
        scrollViewRoot.SetActive(false);
        isVisible = false;
    }

    public void ToggleVisibility()
    {
        isVisible = !isVisible;
        scrollViewRoot.SetActive(isVisible);
    }

    public bool IsVisible() => isVisible;
}