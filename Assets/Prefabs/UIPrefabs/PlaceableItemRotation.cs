using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlaceableItemRotation : MonoBehaviour
{
    [SerializeField] private GameObject rotationButtonPrefab;
    [SerializeField] private float buttonDistance = 1.5f;
    [SerializeField] private float heightOffset = 1f;
    
    private GameObject[] rotationButtons = new GameObject[8];
    private GeneralSessionManager gameManager;
    private ContextMenu3D contextMenu;
    
    void Start()
    {
        gameManager = FindObjectOfType<GeneralSessionManager>();
        CreateRotationButtons();
        SetButtonsVisibility(false);
    }
    

    
    void CreateRotationButtons()
    {
        if (rotationButtonPrefab == null)
        {
            Debug.LogWarning("Rotation button prefab not assigned!");
            return;
        }
        
        Vector3 parentScale = transform.localScale;
        Vector3 inverseScale = new Vector3(1f / parentScale.x, 1f / parentScale.y, 1f / parentScale.z);
        
        float scaledDistance = buttonDistance * inverseScale.x;
        float scaledHeight = heightOffset * inverseScale.y;
        
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            Vector3 position = new Vector3(Mathf.Sin(angle) * scaledDistance, scaledHeight, Mathf.Cos(angle) * scaledDistance);
            
            GameObject button = Instantiate(rotationButtonPrefab);
            button.transform.SetParent(transform);
            button.transform.localPosition = position;
            button.transform.localRotation = Quaternion.Euler(90, i * 45 + 135 - 90, 0);
            button.transform.localScale = Vector3.Scale(Vector3.one * 6.0f, inverseScale);
            
            Vector3 localOffset = new Vector3(0.5f, 0, 0);
            button.transform.localPosition += button.transform.TransformDirection(localOffset);
            button.name = $"RotationButton_{i * 45}";
            
            if (button.GetComponent<Collider>() == null)
            {
                BoxCollider box = button.AddComponent<BoxCollider>();
                box.size = new Vector3(2f, 2f, 0.1f);
                box.center = new Vector3(0.5f, 0.5f, 0);
            }
            
            int direction = i * 45;
            var clickHandler = button.AddComponent<SimpleClickHandler>();
            clickHandler.OnClick = () => RotateToDirection(direction);
            
            rotationButtons[i] = button;
        }
    }
    
    public void ShowButtons()
    {
        SetButtonsVisibility(true);
    }
    
    public void HideButtons()
    {
        SetButtonsVisibility(false);
    }
    
    private void SetButtonsVisibility(bool visible)
    {
        foreach (GameObject button in rotationButtons)
        {
            if (button != null)
                button.SetActive(visible);
        }
    }
    
    void RotateToDirection(int targetAngle)
    {
        PlaceableItemInstance targetObject = GetComponentInParent<PlaceableItemInstance>();
        
        if (targetObject != null)
        {
            float currentY = targetObject.transform.eulerAngles.y;
            float newDirection = (currentY + targetAngle) % 360;
            targetObject.transform.rotation = Quaternion.Euler(0, newDirection, 0);
        }
    }
}

public class SimpleClickHandler : MonoBehaviour
{
    public System.Action OnClick;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            {
                OnClick?.Invoke();
            }
        }
    }
}