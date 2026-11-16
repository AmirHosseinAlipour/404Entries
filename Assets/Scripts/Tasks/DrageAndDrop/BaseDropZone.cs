using UnityEngine;
using UnityEngine.EventSystems;

public class BaseDropZone : MonoBehaviour, IDropHandler
{
    public virtual void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null) return;
        BaseDraggableItem draggable = eventData.pointerDrag.GetComponent<BaseDraggableItem>();

        if (draggable != null && !draggable.isLocked)
        {
            draggable.NotifyDrop(this);
        }
    }
}
