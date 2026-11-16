using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BaseDraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static event Action OnItemDropCompleted;
    public BaseDropZone currentDropZone { get; set; }
    
    public RectTransform rectTransform;
    protected Image image;
    protected Canvas canvas;

    protected Vector2 originalPosition;
    protected Transform originalParent;
    protected Vector3 originalLocalScale = Vector3.one;

    public bool isLocked = false;
    protected bool wasDropped = false;

    protected virtual void Awake()
    {
        EnsureReferences();
    }

    public void EnsureReferences()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (image == null) image = GetComponent<Image>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>(true);
        if (canvas == null)
        {
            Canvas[] allCanvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
            foreach (var c in allCanvases)
            {
                if (c != null && c.name.Contains("Mission"))
                {
                    canvas = c;
                    break;
                }
            }
            if (canvas == null && allCanvases.Length > 0)
            {
                canvas = allCanvases[0];
            }
        }
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        EnsureReferences();
        if (isLocked)
        {
            if (eventData != null) eventData.pointerDrag = null;
            return;
        }

        originalPosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
        originalParent = transform.parent;
        originalLocalScale = transform.localScale;

        if (canvas != null)
        {
            transform.SetParent(canvas.transform, true);
            transform.SetAsLastSibling();
        }
        else
        {
            transform.SetAsLastSibling();
        }

        if (image != null) image.raycastTarget = false;
        wasDropped = false;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        EnsureReferences();
        if (canvas != null && transform.parent != canvas.transform)
            return;

        if (rectTransform != null && eventData != null)
        {
            float scale = (canvas != null && canvas.scaleFactor > 0.001f) ? canvas.scaleFactor : 1f;
            rectTransform.anchoredPosition += eventData.delta / scale;
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        EnsureReferences();
        if (image != null) image.raycastTarget = true;

        if (!wasDropped && canvas != null && transform.parent == canvas.transform)
        {
            ReturnToOriginalPosition();
        }
        else if (!wasDropped && originalParent != null && transform.parent != originalParent)
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
        if (zone != null)
        {
            transform.SetParent(zone.transform, false);
            if (rectTransform != null) rectTransform.anchoredPosition = Vector2.zero;
            currentDropZone = zone;
        }
    }

    public virtual void ReturnToOriginalPosition()
    {
        currentDropZone = null;
        if (originalParent != null)
        {
            transform.SetParent(originalParent, false);
        }
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
        transform.localScale = originalLocalScale;
    }
}
