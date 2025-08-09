using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableAction : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    private Vector2 originalPosition;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private Vector2 pointerOffset;
    private RectTransform rectTransform;

    public ActionScrollViewManager OriginManager;
    public PlaceableItemInstance Unit;
    public string ActionDescription;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        canvasGroup.blocksRaycasts = false;

        // Reparent to root canvas (preserve world position)
        transform.SetParent(rootCanvas.transform, true);

        // Get local position in root canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 globalMousePos);

        pointerOffset = rectTransform.anchoredPosition - globalMousePos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 globalMousePos))
        {
            rectTransform.anchoredPosition = globalMousePos + pointerOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        Vector2 screenPoint = eventData.position;
        bool droppedInScrollView = false;

        foreach (var scrollView in ActionScrollViewManager.AllScrollViews)
        {
            if (scrollView.IsPointInScrollViewA(screenPoint))
            {
                if (transform.parent == scrollView.ContentPanelA)
                {
                    // Already in A — just reset position
                    transform.SetParent(scrollView.ContentPanelA, false);
                    rectTransform.anchoredPosition = originalPosition;
                    droppedInScrollView = true;
                    break;
                }

                // Move to ScrollView A
                MoveToScrollView(scrollView, true);
                droppedInScrollView = true;
                break;
            }
            else if (scrollView.IsPointInScrollViewB(screenPoint))
            {
                if (transform.parent == scrollView.ContentPanelB)
                {
                    // Already in B — just reset position
                    transform.SetParent(scrollView.ContentPanelB, false);
                    rectTransform.anchoredPosition = originalPosition;
                    droppedInScrollView = true;
                    break;
                }

                // Move to ScrollView B
                MoveToScrollView(scrollView, false);
                droppedInScrollView = true;
                break;
            }
        }

        if (!droppedInScrollView)
        {
            // Return to original position
            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    private void MoveToScrollView(ActionScrollViewManager targetScrollView, bool toScrollViewA)
    {
        // Remove from old manager list
        OriginManager?.RemoveActionFromInternalLists(Unit, ActionDescription);

        // Change parent of dragged object
        Transform newParent = toScrollViewA ? targetScrollView.ContentPanelA : targetScrollView.ContentPanelB;
        transform.SetParent(newParent, false);
        rectTransform.anchoredPosition = Vector2.zero;

        // Update OriginManager to new scrollview
        OriginManager = targetScrollView;

        // Add this entry to target scrollview internal list
        OriginManager.AddExistingEntry(new ActionScrollViewManager.ActionEntry
        {
            Unit = Unit,
            Team = Unit.getTeam(),
            ActionDescription = ActionDescription,
            DisplayObject = gameObject
        });
    }
}