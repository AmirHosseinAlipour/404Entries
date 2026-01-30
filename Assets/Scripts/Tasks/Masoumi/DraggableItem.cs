using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : BaseDraggableItem
{
    private DropZone pendingDropZone = null;

    public override void OnBeginDrag(PointerEventData eventData)
    {
        // Derived class re-implements the lock check
        if (isLocked)
        {
            eventData.pointerDrag = null;
            return;
        }
        
        base.OnBeginDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;

        if (pendingDropZone != null)
        {
            PaperManager.Instance.ShowConfirmation(this, pendingDropZone);
        }
        else if (transform.parent == canvas.transform) 
        {
            ReturnToOriginalPosition();
        }
    }

    public override void NotifyDrop(BaseDropZone zone)
    {
        DropZone confirmationZone = zone as DropZone;

        if (confirmationZone != null)
        {
            pendingDropZone = confirmationZone;
            wasDropped = true;
        }
        else
        {
            base.NotifyDrop(zone);
        }
    }

    public void CancelDrop()
    {
        ReturnToOriginalPosition();
        pendingDropZone = null;
        wasDropped = false;
    }

    public void CompleteDrop()
    {
        if (pendingDropZone == null) return;

        transform.rotation = Quaternion.identity;
        FinalizeDrop(pendingDropZone);
        
        // Locking is now handled here, in the derived class
        isLocked = true; 
        
        pendingDropZone = null;
    }
}