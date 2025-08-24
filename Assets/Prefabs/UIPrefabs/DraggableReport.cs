using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableReport : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    private Vector2 originalPosition;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private Vector2 pointerOffset;
    private RectTransform rectTransform;

    public ReportScrollViewManagerGameMasterIncoming OriginManager;
    public ReportEntry ReportData;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        canvasGroup.blocksRaycasts = false;

        // Reparent to root canvas for free dragging
        transform.SetParent(rootCanvas.transform, true);

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

        foreach (var scrollView in ReportScrollViewManagerGameMasterIncoming.AllScrollViews)
        {
            if (scrollView.IsPointInScrollViewA(screenPoint))
            {
                if (transform.parent == scrollView.ContentPanelA)
                {
                    transform.SetParent(scrollView.ContentPanelA, false);
                    rectTransform.anchoredPosition = originalPosition;
                    droppedInScrollView = true;
                    break;
                }

                MoveToScrollView(scrollView, 0);
                droppedInScrollView = true;
                break;
            }
            else if (scrollView.IsPointInScrollViewB(screenPoint))
            {
                if (transform.parent == scrollView.ContentPanelB)
                {
                    transform.SetParent(scrollView.ContentPanelB, false);
                    rectTransform.anchoredPosition = originalPosition;
                    droppedInScrollView = true;
                    break;
                }

                MoveToScrollView(scrollView, 1);
                droppedInScrollView = true;
                break;
            }
            else if (scrollView.IsPointInScrollViewC(screenPoint))
            {
                if (transform.parent == scrollView.ContentPanelC)
                {
                    transform.SetParent(scrollView.ContentPanelC, false);
                    rectTransform.anchoredPosition = originalPosition;
                    droppedInScrollView = true;
                    break;
                }

                MoveToScrollView(scrollView, 2);
                droppedInScrollView = true;
                break;
            }
        }

        if (!droppedInScrollView)
        {
            // Return to original place if not dropped in valid scrollview
            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    private void MoveToScrollView(ReportScrollViewManagerGameMasterIncoming targetScrollView, int targetPanel)
    {
        OriginManager?.RemoveReport(ReportData);

        Transform newParent = targetPanel switch
        {
            0 => targetScrollView.ContentPanelA,
            1 => targetScrollView.ContentPanelB,
            2 => targetScrollView.ContentPanelC,
            _ => targetScrollView.ContentPanelA
        };
        
        transform.SetParent(newParent, false);
        rectTransform.anchoredPosition = Vector2.zero;

        OriginManager = targetScrollView;
        OriginManager.AddExistingEntry(ReportData, targetPanel);
    }
}