using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Image image;
    private Vector2 originalPosition;
    private Transform originalParent;
    private Canvas canvas;

    private DropZone pendingDropZone = null;
    public bool isLocked = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked)
        {
            eventData.pointerDrag = null;
            return;
        }

        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();
        
        image.raycastTarget = false;
        pendingDropZone = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked || transform.parent != canvas.transform)
            return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;

        if (pendingDropZone != null)
        {
            PaperManager.Instance.ShowConfirmation(this, pendingDropZone);
        }
        else if (transform.parent == canvas.transform)
        {
            CancelDrop();
        }
    }

    public void SetPendingDrop(DropZone zone)
    {
        pendingDropZone = zone;
    }

    public void CancelDrop()
    {
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = originalPosition;
        pendingDropZone = null;
    }

    public void CompleteDrop()
    {
        if (pendingDropZone == null) return;

        transform.rotation = Quaternion.identity;
        transform.SetParent(pendingDropZone.transform);
        rectTransform.anchoredPosition = Vector2.zero;
        isLocked = true;
        pendingDropZone = null;
    }
}
