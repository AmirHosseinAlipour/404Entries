using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public bool isPassSlot;
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        DraggableItem draggableItem = droppedObject.GetComponent<DraggableItem>();

        if (draggableItem != null && !draggableItem.isLocked)
        {
            draggableItem.SetPendingDrop(this);
        }
    }
}