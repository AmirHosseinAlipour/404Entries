using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BaseDraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static event Action OnItemDropCompleted;
    public BaseDropZone currentDropZone { get; protected set; }
    
    protected RectTransform rectTransform;
    protected Image image;
    protected Canvas canvas;

    protected Vector2 originalPosition;
    protected Transform originalParent;

    public bool isLocked = false;
    protected bool wasDropped = false;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        // Lock check is removed from base class
        
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();
        
        image.raycastTarget = false;
        wasDropped = false;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (transform.parent != canvas.transform)
            return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;

        if (!wasDropped && transform.parent == canvas.transform)
        {
            ReturnToOriginalPosition();
        }
    }

    public virtual void NotifyDrop(BaseDropZone zone)
    {
        FinalizeDrop(zone);
        wasDropped = true;
        
        OnItemDropCompleted?.Invoke();
    }

    public virtual void FinalizeDrop(BaseDropZone zone)
    {
        transform.SetParent(zone.transform, false);
        rectTransform.anchoredPosition = Vector2.zero;
        currentDropZone = zone;
    }

    public virtual void ReturnToOriginalPosition()
    {
        currentDropZone = null;
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = originalPosition;
    }
}