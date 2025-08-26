using UnityEngine;
using UnityEngine.EventSystems;

public class PlaceableItemClickHandler : MonoBehaviour
{
    void Update()
    {
        // Detect left mouse button click
        if (Input.GetMouseButtonDown(0))
        {
            // Debug.Log($"[PlaceableItemClickHandler] Left mouse button clicked");
            
            // If pointer is over UI, do NOT process click
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // Debug.Log($"[PlaceableItemClickHandler] Click ignored - pointer over UI");
                return;
            }

            // Cast a ray from camera through mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // Debug.Log($"[PlaceableItemClickHandler] Casting ray from mouse position: {Input.mousePosition}");

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Debug.Log($"[PlaceableItemClickHandler] Raycast hit: {hit.collider.name}");
                
                // Check if the hit object has a PlaceableItemInstance component
                PlaceableItemInstance placeable = hit.collider.GetComponent<PlaceableItemInstance>();
                
                // If not found on hit object, check parent objects
                if (placeable == null)
                {
                    placeable = hit.collider.GetComponentInParent<PlaceableItemInstance>();
                    // Debug.Log($"[PlaceableItemClickHandler] Checking parent for PlaceableItemInstance: {(placeable != null ? "Found" : "Not found")}");
                }

                if (placeable != null)
                {
                    // Debug.Log($"[PlaceableItemClickHandler] Found PlaceableItemInstance on {placeable.gameObject.name}, calling OnClicked()");
                    placeable.OnClicked();
                }
                else
                {
                    // Debug.Log($"[PlaceableItemClickHandler] No PlaceableItemInstance found on {hit.collider.name} or its parents");
                }
            }
            else
            {
                // Debug.Log($"[PlaceableItemClickHandler] Raycast missed - no hit detected");
            }
        }
    }
}