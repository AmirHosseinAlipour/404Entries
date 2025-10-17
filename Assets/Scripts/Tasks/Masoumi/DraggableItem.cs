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

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
    }

    // Called when the drag is first detected
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Store original state
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        
        // Make the item a direct child of the canvas to render on top of everything
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();
        
        image.raycastTarget = false;
    }

    // Called every frame while the object is being dragged
    public void OnDrag(PointerEventData eventData)
    {
        // Update the position of the UI element to follow the mouse/finger
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    // Called when the drag is released
    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;

        // If the item's parent is still the canvas, it means it wasn't dropped on a valid zone
        if (transform.parent == canvas.transform)
        {
            // Snap back to its original parent and position
            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}