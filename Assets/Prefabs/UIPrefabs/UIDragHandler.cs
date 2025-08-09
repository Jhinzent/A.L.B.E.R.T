using UnityEngine;
using UnityEngine.EventSystems;

public class DragPopup : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    private RectTransform popupRectTransform;
    private Canvas canvas;
    private Vector2 pointerOffset;

    private void Awake()
    {
        popupRectTransform = GetComponentInParent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            Debug.LogError("DragPopup: No Canvas found in parent hierarchy.");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Use parent as the coordinate reference (usually the popup container's parent)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            popupRectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPointerPosition);

        // Correctly calculate the offset between pointer and popup's anchored position
        pointerOffset = popupRectTransform.anchoredPosition - localPointerPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            popupRectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPointerPosition))
        {
            popupRectTransform.anchoredPosition = localPointerPosition + pointerOffset;
        }
    }
}