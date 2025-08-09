using UnityEngine;
using UnityEngine.EventSystems;

public class PlaceableItemClickHandler : MonoBehaviour
{
    void Update()
    {
        // Detect left mouse button click
        if (Input.GetMouseButtonDown(0))
        {
            // If pointer is over UI, do NOT process click
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            // Cast a ray from camera through mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Check if the hit object has a PlaceableItemInstance component
                PlaceableItemInstance placeable = hit.collider.GetComponent<PlaceableItemInstance>();

                if (placeable != null)
                {
                    placeable.OnClicked();
                }
            }
        }
    }
}