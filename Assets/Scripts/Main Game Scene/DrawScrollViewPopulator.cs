using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorScrollPopulator : MonoBehaviour
{
    [SerializeField] private Transform[] contentParents; // ScrollView/Viewport/Content for each player
    [SerializeField] private GameObject colorButtonPrefab;
    [SerializeField] private GameObject[] scrollViewRoots; // Root GameObject of each scroll view
    [SerializeField] private GameObject deleteAllButtonPrefab;

    private Color[] colors = new Color[]
    {
        Color.black, Color.white, Color.red, Color.green, Color.blue,
        Color.yellow, Color.cyan, Color.magenta, new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 1f) // some extras
    };

    private bool isVisible = true;

    private void Start()
    {
        for (int i = 0; i < contentParents.Length; i++)
        {
            PopulateScrollView(contentParents[i], scrollViewRoots[i]);
        }
    }
    
    private void PopulateScrollView(Transform contentParent, GameObject scrollViewRoot)
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
            if (DrawMeshFull.Instance != null)
                DrawMeshFull.Instance.ClearAllDrawings();
        });

        // Optional: Start with hidden scroll view
        scrollViewRoot.SetActive(false);
    }

    public void ToggleVisibility(int playerIndex = 0)
    {
        if (playerIndex < scrollViewRoots.Length)
        {
            isVisible = !isVisible;
            scrollViewRoots[playerIndex].SetActive(isVisible);
        }
    }

    public bool IsVisible() => isVisible;
}