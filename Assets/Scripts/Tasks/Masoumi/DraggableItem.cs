using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : BaseDraggableItem
{
    private DropZone pendingDropZone = null;

    public override void OnBeginDrag(PointerEventData eventData)
    {
        EnsureReferences();
        if (isLocked)
        {
            if (eventData != null) eventData.pointerDrag = null;
            return;
        }
        
        base.OnBeginDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        EnsureReferences();
        if (image != null) image.raycastTarget = true;

        if (pendingDropZone != null)
        {
            if (PaperManager.Instance != null)
            {
                PaperManager.Instance.ShowConfirmation(this, pendingDropZone);
            }
            else
            {
                CompleteDrop();
            }
        }
        else if (canvas != null && transform.parent == canvas.transform) 
        {
            ReturnToOriginalPosition();
        }
        else if (originalParent != null && transform.parent != originalParent)
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
        
        isLocked = true; 
        pendingDropZone = null;
    }
}
